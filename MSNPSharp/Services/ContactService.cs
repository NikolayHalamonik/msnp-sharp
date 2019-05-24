#region
/*
Copyright (c) 2002-2012, Bas Geertsema, Xih Solutions
(http://www.xihsolutions.net), Thiago.Sayao, Pang Wu, Ethem Evlice, Andy Phan, Chang Liu. 
All rights reserved. http://code.google.com/p/msnp-sharp/

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice,
  this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.
* Neither the names of Bas Geertsema or Xih Solutions nor the names of its
  contributors may be used to endorse or promote products derived from this
  software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 'AS IS'
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Text.RegularExpressions;

namespace MSNPSharp
{
    using MSNPSharp.IO;
    using MSNPSharp.Core;
    using MSNPSharp.MSNWS.MSNABSharingService;
    using MSNPSharp.MSNWS.MSNDirectoryService;

    /// <summary>
    /// Provide webservice operations for contacts. This class cannot be inherited.
    /// </summary>
    public sealed partial class ContactService : MSNService
    {
        #region Fields

        private bool abSynchronized;
        private object syncObject = new object();
        private Semaphore binarySemaphore = new Semaphore(1, 1);

        internal XMLContactList AddressBook;
        internal DeltasList Deltas;

        #endregion

        public ContactService(NSMessageHandler nsHandler)
            : base(nsHandler)
        {
        }

        #region Events

        /// <summary>
        /// Fires when a call to SynchronizeList() has been made and the synchronization process is completed.
        /// This means all contact-updates are received from the server and processed.
        /// </summary>
        public event EventHandler<EventArgs> SynchronizationCompleted;
        internal void OnSynchronizationCompleted(EventArgs e)
        {
            abSynchronized = true;

            if (SynchronizationCompleted != null)
                SynchronizationCompleted(this, e);
        }

        /// <summary>
        /// Fires when a contact is added to pending list
        /// </summary>
        public event EventHandler<ContactEventArgs> FriendshipRequested;
        internal void OnFriendshipRequested(ContactEventArgs e)
        {
            if (FriendshipRequested != null)
                FriendshipRequested(this, e);
        }

        /// <summary>
        /// Fires when a contact is added to any list (including reverse list)
        /// </summary>
        public event EventHandler<ListMutateEventArgs> ContactAdded;
        internal void OnContactAdded(ListMutateEventArgs e)
        {
            if (ContactAdded != null)
                ContactAdded(this, e);
        }

        /// <summary>
        /// Fires when a contact is removed from any list (including reverse list)
        /// </summary>
        public event EventHandler<ListMutateEventArgs> ContactRemoved;
        internal void OnContactRemoved(ListMutateEventArgs e)
        {
            if (ContactRemoved != null)
                ContactRemoved(this, e);
        }

        /// <summary>
        /// Fires when a new contactgroup is created
        /// </summary>
        public event EventHandler<ContactGroupEventArgs> ContactGroupAdded;
        internal void OnContactGroupAdded(ContactGroupEventArgs e)
        {
            if (ContactGroupAdded != null)
                ContactGroupAdded(this, e);
        }

        /// <summary>
        /// Fires when a contactgroup is removed
        /// </summary>
        public event EventHandler<ContactGroupEventArgs> ContactGroupRemoved;
        internal void OnContactGroupRemoved(ContactGroupEventArgs e)
        {
            if (ContactGroupRemoved != null)
                ContactGroupRemoved(this, e);
        }

        /// <summary>
        /// Fires when a new <see cref="Contact"/> is created.
        /// </summary>
        public event EventHandler<CircleEventArgs> CreateCircleCompleted;
        internal void OnCreateCircleCompleted(CircleEventArgs e)
        {
            if (CreateCircleCompleted != null)
                CreateCircleCompleted(this, e);
        }

        /// <summary>
        /// Fires when the owner has left a specific <see cref="Contact"/>.
        /// </summary>
        public event EventHandler<CircleEventArgs> ExitCircleCompleted;
        internal void OnExitCircleCompleted(CircleEventArgs e)
        {
            Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Exit circle completed: " + e.Circle.ToString());

            if (ExitCircleCompleted != null)
                ExitCircleCompleted(this, e);
        }

        /// <summary>
        /// Fired after the InviteContactToCircle succeeded.
        /// </summary>
        public event EventHandler<CircleMemberEventArgs> InviteCircleMemberCompleted;
        internal void OnInviteCircleMemberCompleted(CircleMemberEventArgs e)
        {
            if (InviteCircleMemberCompleted != null)
                InviteCircleMemberCompleted(this, e);
        }

        /// <summary>
        /// Fired after a circle member has left the circle.
        /// </summary>
        public event EventHandler<CircleMemberEventArgs> CircleMemberLeft;
        internal void OnCircleMemberLeft(CircleMemberEventArgs e)
        {
            if (CircleMemberLeft != null)
                CircleMemberLeft(this, e);
        }

        /// <summary>
        /// Fired after a circle member has joined the circle.
        /// </summary>
        public event EventHandler<CircleMemberEventArgs> CircleMemberJoined;
        internal void OnCircleMemberJoined(CircleMemberEventArgs e)
        {
            if (CircleMemberJoined != null)
                CircleMemberJoined(this, e);
        }

        /// <summary>
        /// Fired after a remote user invite us to join a circle.
        /// </summary>
        public event EventHandler<CircleEventArgs> JoinCircleInvitationReceived;
        internal void OnJoinCircleInvitationReceived(CircleEventArgs e)
        {
            if (JoinCircleInvitationReceived != null)
                JoinCircleInvitationReceived(this, e);
        }

        /// <summary>
        /// Fired after the owner join a circle successfully.
        /// </summary>
        public event EventHandler<CircleEventArgs> JoinedCircleCompleted;
        internal void OnJoinedCircleCompleted(CircleEventArgs e)
        {
            Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Circle invitation accepted: " + e.Circle.ToString());

            if (JoinedCircleCompleted != null)
                JoinedCircleCompleted(this, e);
        }

        #endregion

        #region Properties

        public object SyncObject
        {
            get
            {
                return syncObject;
            }
        }

        public Semaphore BinarySemaphore
        {
            get
            {
                return binarySemaphore;
            }
        }

        /// <summary>
        /// Keep track whether a address book synchronization has been completed.
        /// </summary>
        public bool AddressBookSynchronized
        {
            get
            {
                return abSynchronized;
            }
        }

        #endregion

        #region Synchronize

        /// <summary>
        /// Rebuild the contactlist with the most recent data.
        /// </summary>
        /// <remarks>
        /// Synchronizing is the most complete way to retrieve data about groups, contacts, privacy settings, etc.
        /// This method is called automatically after owner profile received and then the addressbook is merged with deltas file.
        /// After that, SignedIn event occurs and the client programmer must set it's initial status by SetPresenceStatus(). 
        /// Otherwise you won't receive online notifications from other clients or the connection is closed by the server.
        /// If you have an external contact list, you must track ProfileReceived, SignedIn and SynchronizationCompleted events.
        /// Between ProfileReceived and SignedIn: the internal addressbook is merged with deltas file.
        /// Between SignedIn and SynchronizationCompleted: the internal addressbook is merged with most recent data by soap request.
        /// All contact changes will be fired between ProfileReceived, SignedIn and SynchronizationCompleted events. 
        /// e.g: ContactAdded, ContactRemoved, ReverseAdded, ReverseRemoved.
        /// </remarks>
        internal void SynchronizeContactList()
        {
            if (AddressBookSynchronized)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceWarning, "SynchronizeContactList() was called, but the list has already been synchronized.", GetType().Name);
                return;
            }

            MclSerialization st = Settings.SerializationType;
            string addressbookFile = Path.Combine(Settings.SavePath, NSMessageHandler.Owner.Account.GetHashCode() + ".mcl");
            string deltasResultsFile = Path.Combine(Settings.SavePath, NSMessageHandler.Owner.Account.GetHashCode() + "d" + ".mcl");

            BinarySemaphore.WaitOne();
            try
            {
                AddressBook = XMLContactList.LoadFromFile(addressbookFile, st, NSMessageHandler, false);
                Deltas = DeltasList.LoadFromFile(deltasResultsFile, st, NSMessageHandler.Credentials.Password, true);
            }
            catch (Exception)
            {
                // InvalidOperationException: Struct changed (Serialize error)
                DeleteRecordFile(true); // Reset addressbook.
            }
            finally
            {
                BinarySemaphore.Release();
            }

            try
            {
                if (AddressBook.Version != XMLContactList.XMLContactListVersion || Deltas.Version != DeltasList.DeltasListVersion)
                {
                    Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo,
                        "Your MCL addressbook version is outdated: " + AddressBook.Version.ToString() +
                        "\r\nAddressBook Version Required: " + XMLContactList.XMLContactListVersion +
                        "\r\nThe old mcl files for this account will be deleted and a new request for getting full addressbook list will be post.");

                    DeleteRecordFile(true); // Addressbook version changed. Reset addressbook.
                    // SOFT ERROR(continue).
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Couldn't delete addressbook: " + ex.Message, GetType().Name);
                return; // HARD ERROR. Don't continue. I am dead.
            }

            // Should has no lock here.
            bool firstTime = false;
            BinarySemaphore.WaitOne();
            try
            {
                if (AddressBook != null)
                {
                    AddressBook.Initialize();
                    firstTime = (DateTime.MinValue == WebServiceDateTimeConverter.ConvertToDateTime(AddressBook.GetAddressBookLastChange(WebServiceConstants.MessengerIndividualAddressBookId)));
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                BinarySemaphore.Release();
            }

            if (firstTime)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo,
                    "Getting your membership list for the first time. If you have a lot of contacts, please be patient!", GetType().Name);
            }

            // Should be no lock here, let the msRequest take care of the locks.
            try
            {
                msRequest(
                    PartnerScenario.Initial,
                    delegate
                    {
                        BinarySemaphore.WaitOne();
                        {
                            if (AddressBook != null && Deltas != null)
                            {
                                BinarySemaphore.Release();

                                if (firstTime)
                                {
                                    Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo,
                                        "Getting your address book for the first time. If you have a lot of contacts, please be patient!", GetType().Name);
                                }

                                // Should be no lock here, let the abRequest take care of the locks.
                                try
                                {
                                    abRequest(PartnerScenario.Initial,
                                        delegate
                                        {
                                            // Should be no lock here, let the InitialABRequestCompleted take care of the locks.
                                            InitialMembershipAndAbRequestCompleted();
                                        }
                                    );
                                }
                                catch (Exception abRequestEception)
                                {
                                    OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABFindContactsPaged",
                                        new MSNPSharpException(abRequestEception.Message, abRequestEception)));

                                    Trace.WriteLineIf(Settings.TraceSwitch.TraceError,
                                        "An error occured while getting membership list: " + abRequestEception.Message, GetType().Name);
                                    return;
                                }
                            }
                            else
                            {
                                //If addressbook is null, we are still locking.
                                BinarySemaphore.Release();
                            }
                        }
                        // Should has no lock here.
                    }
                );
            }
            catch (Exception ex)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("FindMembership", new MSNPSharpException(ex.Message, ex)));
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "An error occured while getting membership list: " + ex.Message, GetType().Name);
            }
        }

        private void InitialMembershipAndAbRequestCompleted()
        {
            BinarySemaphore.WaitOne();
            try
            {
                if (AddressBook != null && Deltas != null)
                {
                    // Save addressbook and then truncate deltas file.
                    AddressBook.Save();
                    Deltas.Truncate();
                }
            }
            finally
            {
                BinarySemaphore.Release();
            }

            NSMessageHandler.SetDefaults();
        }

        /// <summary>
        /// Async membership request
        /// </summary>
        /// <param name="partnerScenario"></param>
        /// <param name="onSuccess">The delegate to be executed after async membership request completed successfuly</param>
        internal void msRequest(string partnerScenario, FindMembershipCompletedEventHandler onSuccess)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("FindMembership", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }


            FindMembershipAsync(partnerScenario,
                // Register callback for success/error. e.Cancelled handled by FindMembershipAsync.
                delegate(object sender, FindMembershipCompletedEventArgs fmcea)
                {
                    if (fmcea.Error == null /* No error */)
                    {
                        BinarySemaphore.WaitOne();

                        // Addressbook re-defined here, because the reference can be changed.
                        // FindMembershipAsync can delete addressbook if addressbook sync is required.
                        XMLContactList xmlcl;
                        MembershipResult fmResult;

                        if ((null != (xmlcl = AddressBook)) &&
                            (null != (fmResult = fmcea.Result.FindMembershipResult)))
                        {
                            BinarySemaphore.Release();
                            try
                            {
                                // Following line is horrible for semaphore usage...
                                xmlcl
                                    .Merge(fmResult)
                                    .Save();
                            }
                            catch (Exception unknownException)
                            {
                                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("FindMembership",
                                    new MSNPSharpException("Unknown Exception occurred while synchronizing contact list, please see inner exception.",
                                    unknownException)));
                            }
                        }
                        else
                        {
                            BinarySemaphore.Release();
                        }

                        BinarySemaphore.WaitOne();
                        if (AddressBook != null && Deltas != null)
                        {
                            BinarySemaphore.Release();

                            if (onSuccess != null)
                            {
                                onSuccess(sender, fmcea);
                            }
                        }
                        else
                        {
                            BinarySemaphore.Release();

                            OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("FindMembership",
                                    new MSNPSharpException("Addressbook and Deltas have been reset.")));
                        }
                    }
                    else
                    {
                        // Error handler
                        BinarySemaphore.WaitOne();
                        {
                            if (AddressBook == null && Deltas == null && AddressBookSynchronized == false)
                            {
                                // This means before the webservice returned the connection had broken.
                                BinarySemaphore.Release();
                                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("FindMembership",
                                        new MSNPSharpException("Addressbook and Deltas have been reset.")));
                                return;
                            }
                        }
                        BinarySemaphore.Release();
                    }
                }
            );
        }

        /// <summary>
        /// Async Address book request
        /// </summary>
        /// <param name="partnerScenario"></param>
        /// <param name="onSuccess">The delegate to be executed after async ab request completed successfuly</param>
        internal void abRequest(string partnerScenario, ABFindContactsPagedCompletedEventHandler onSuccess)
        {
            try
            {
                abRequest(partnerScenario, null, onSuccess);
            }
            catch (Exception ex)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABFindContactsPaged",
                   new MSNPSharpException(ex.Message, ex)));
                return;
            }
        }

        /// <summary>
        /// Async Address book request
        /// </summary>
        /// <param name="partnerScenario"></param>
        /// <param name="abHandle">The specified addressbook to retrieve.</param>
        /// <param name="onSuccess">The delegate to be executed after async ab request completed successfuly</param>
        internal void abRequest(string partnerScenario, abHandleType abHandle, ABFindContactsPagedCompletedEventHandler onSuccess)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABFindContactsPaged", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            ABFindContactsPagedAsync(partnerScenario, abHandle,
                // Register callback for success/error. e.Cancelled handled by ABFindContactsPagedAsync.
                delegate(object sender, ABFindContactsPagedCompletedEventArgs abfcpcea)
                {
                    if (abfcpcea.Error == null /* No error */)
                    {
                        BinarySemaphore.WaitOne();

                        // Addressbook re-defined here, because the reference can be changed.
                        // ABFindContactsPagedAsync can delete addressbook if addressbook sync is required.
                        XMLContactList xmlcl;
                        ABFindContactsPagedResultType forwardList;
                        String circleResult = null;

                        if ((null != (xmlcl = AddressBook)) &&
                            (null != (forwardList = abfcpcea.Result.ABFindContactsPagedResult)))
                        {
                            // Following line is horrible for semaphore usage...
                            xmlcl
                                .Merge(forwardList)
                                .Save();

                            if (forwardList.CircleResult != null)
                            {
                                circleResult = forwardList.CircleResult.CircleTicket;
                            }
                        }

                        BinarySemaphore.Release();

                        if (!String.IsNullOrEmpty(circleResult))
                        {
                            NSMessageHandler.SendSHAAMessage(circleResult);
                        }

                        if (onSuccess != null)
                        {
                            onSuccess(sender, abfcpcea);
                        }
                    }
                    else
                    {
                        // Error handler

                        BinarySemaphore.WaitOne();
                        if (AddressBook == null && Deltas == null && AddressBookSynchronized == false)
                        {
                            BinarySemaphore.Release();
                            // This means before the webservice returned the connection had broken.
                            OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABFindContactsPaged",
                                    new MSNPSharpException("Addressbook and Deltas have been reset.")));
                            return;
                        }
                        BinarySemaphore.Release();
                    }
                });
        }


        #endregion

        #region Contact & Group Operations

        #region FindFriendsInCommon

        public void FindFriendsInCommon(Contact contact, int count, FindFriendsInCommonCompletedEventHandler onSuccess)
        {
            FindFriendsInCommonAsync(Guid.Empty, contact.CID, count,
                delegate(object service, FindFriendsInCommonCompletedEventArgs ffincea)
                {
                    if (ffincea.Error == null && ffincea.Result != null && ffincea.Result.FindFriendsInCommonResult != null)
                    {
                        if (onSuccess != null)
                            onSuccess(service, ffincea);
                    }
                });
        }

        #endregion

        #region Add Contact

        private void CreateContactAndManageWLConnection(string account, IMAddressInfoType network, string invitation)
        {
            CreateContactAsync(
                account,
                network,
                Guid.Empty,
                delegate(object service, CreateContactCompletedEventArgs cce)
                {
                    if (cce.Error == null)
                    {
                        // Get windows live contact (yes)
                        Contact contact = NSMessageHandler.ContactList.GetContactWithCreate(account, IMAddressInfoType.WindowsLive);
                        contact.Guid = new Guid(cce.Result.CreateContactResult.contactId);

                        if (network == IMAddressInfoType.Telephone)
                            return;

                        ManageWLConnectionAsync(
                            contact.Guid, Guid.Empty, invitation,
                            true, true, 1, RelationshipTypes.IndividualAddressBook, (int)RelationshipState.None,
                            delegate(object wlcSender, ManageWLConnectionCompletedEventArgs mwlce)
                            {
                                string payload = ContactList.GenerateMailListForAdl(contact, RoleLists.Allow | RoleLists.Forward, false);
                                NSMessageHandler.MessageProcessor.SendMessage(new NSPayLoadMessage("ADL", payload));

                                // Get all contacts and send ADL for each contact... Yahoo, Facebook etc.
                                abRequest(PartnerScenario.ContactSave,
                                    delegate
                                    {
                                        msRequest(PartnerScenario.ContactSave,
                                            delegate
                                            {
                                                List<IMAddressInfoType> typesFound = new List<IMAddressInfoType>();
                                                IMAddressInfoType[] addressTypes = (IMAddressInfoType[])Enum.GetValues(typeof(IMAddressInfoType));

                                                foreach (IMAddressInfoType at in addressTypes)
                                                {
                                                    if (NSMessageHandler.ContactList.HasContact(account, at))
                                                    {
                                                        typesFound.Add(at);
                                                    }
                                                }

                                                if (typesFound.Count > 0)
                                                {
                                                    Dictionary<string, RoleLists> hashlist = new Dictionary<string, RoleLists>(2);

                                                    foreach (IMAddressInfoType im in typesFound)
                                                    {
                                                        hashlist.Add(Contact.MakeHash(account, im), RoleLists.Allow | RoleLists.Forward);
                                                    }

                                                    payload = ContactList.GenerateMailListForAdl(hashlist, false)[0];
                                                    NSMessageHandler.MessageProcessor.SendMessage(new NSPayLoadMessage("ADL", payload));
                                                }
                                            });
                                    });
                            });
                    }
                });
        }

        /// <summary>
        /// Creates a new contact on your address book and adds to allowed list if not blocked before.
        /// </summary>
        /// <param name="account">An email address or phone number to add. The email address can be yahoo account.</param>
        /// <remarks>The phone format is +CC1234567890 for phone contact, CC is Country Code</remarks>
        public void AddNewContact(string account)
        {
            AddNewContact(account, String.Empty);
        }

        /// <summary>
        /// Creates a new contact on your address book and adds to allowed list if not blocked before.
        /// </summary>
        /// <param name="account">An email address or phone number to add. The email address can be yahoo account.</param>
        /// <param name="invitation">The reason of the adding contact</param>
        /// <remarks>The phone format is +CC1234567890, CC is Country Code</remarks>
        public void AddNewContact(string account, string invitation)
        {
            long test;
            if (long.TryParse(account, out test) ||
                (account.StartsWith("+") && long.TryParse(account.Substring(1), out test)))
            {
                if (account.StartsWith("00"))
                {
                    account = "+" + account.Substring(2);
                }
                AddNewContact(account, IMAddressInfoType.Telephone, invitation);
            }
            else
            {
                AddNewContact(account, IMAddressInfoType.WindowsLive, invitation);
            }
        }

        internal void AddNewContact(string account, IMAddressInfoType network, string invitation)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("AddContact", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            CreateContactAndManageWLConnection(account, network, invitation);
        }

        #endregion

        #region RemoveContact

        public void RemoveContact(Contact contact, bool block)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABContactDelete", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            contact.OnAllowedList = false;

            if (contact.Guid == Guid.Empty)
                return;

            if (contact.ClientType == IMAddressInfoType.WindowsLive)
            {
                BreakConnectionAsync(contact.Guid, Guid.Empty, block, true,
                    delegate(object sender, BreakConnectionCompletedEventArgs e)
                    {
                        if (e.Error != null)
                        {
                            Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Break connection: " + contact.Guid.ToString("D") + " failed, error: " + e.Error.Message);
                            return;
                        }

                        abRequest(PartnerScenario.ContactSave,
                            delegate
                            {
                                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Delete contact :" + contact.Hash + " completed.");
                            });
                    });
            }
            else
            {
                DeleteContactAsync(contact,
                    delegate(object sender, DeleteContactCompletedEventArgs e)
                    {
                        if (e.Error != null)
                        {
                            Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Delete contact: " + contact.Guid.ToString("D") + " failed, error: " + e.Error.Message);
                            return;
                        }

                        abRequest(PartnerScenario.ContactSave,
                            delegate
                            {
                                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Delete contact :" + contact.Hash + " completed.");
                            });
                    });
            }
        }

        /// <summary>
        /// Remove the specified contact from your forward list.
        /// </summary>
        /// <param name="contact">Contact to remove</param>
        public void RemoveContact(Contact contact)
        {
            RemoveContact(contact, false);
        }

        #endregion

        #region UpdateContact

        internal void UpdateContact(Contact contact, Guid abId, ABContactUpdateCompletedEventHandler onSuccess)
        {
            UpdateContact(contact, abId.ToString("D"), onSuccess);
        }

        internal void UpdateContact(Contact contact, string abId, ABContactUpdateCompletedEventHandler onSuccess)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABContactUpdate", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            if (contact.Guid == Guid.Empty)
                throw new InvalidOperationException("This is not a valid Messenger contact.");

            string lowerId = abId.ToLowerInvariant();

            if (!AddressBook.HasContact(lowerId, contact.Guid))
                return;

            ContactType abContactType = AddressBook.SelectContactFromAddressBook(lowerId, contact.Guid);
            ContactType contactToChange = new ContactType();

            List<string> propertiesChanged = new List<string>();

            contactToChange.contactId = contact.Guid.ToString();
            contactToChange.contactInfo = new contactInfoType();

            // Comment
            if (abContactType.contactInfo.comment != contact.Comment)
            {
                propertiesChanged.Add(PropertyString.Comment);
                contactToChange.contactInfo.comment = contact.Comment;
            }

            // DisplayName
            if (abContactType.contactInfo.displayName != contact.Name)
            {
                propertiesChanged.Add(PropertyString.DisplayName);
                contactToChange.contactInfo.displayName = contact.Name;
            }

            //HasSpace
            if (abContactType.contactInfo.hasSpace != contact.HasSpace && abContactType.contactInfo.hasSpaceSpecified)
            {
                propertiesChanged.Add(PropertyString.HasSpace);
                contactToChange.contactInfo.hasSpace = contact.HasSpace;
            }

            // Annotations
            List<Annotation> annotationsChanged = new List<Annotation>();
            Dictionary<string, string> oldAnnotations = new Dictionary<string, string>();
            if (abContactType.contactInfo.annotations != null)
            {
                foreach (Annotation anno in abContactType.contactInfo.annotations)
                {
                    oldAnnotations[anno.Name] = anno.Value;
                }
            }

            // Annotations: AB.NickName
            string oldNickName = oldAnnotations.ContainsKey(AnnotationNames.AB_NickName) ? oldAnnotations[AnnotationNames.AB_NickName] : String.Empty;
            if (oldNickName != contact.NickName)
            {
                Annotation anno = new Annotation();
                anno.Name = AnnotationNames.AB_NickName;
                anno.Value = contact.NickName;
                annotationsChanged.Add(anno);
            }

            if (annotationsChanged.Count > 0)
            {
                propertiesChanged.Add(PropertyString.Annotation);
                contactToChange.contactInfo.annotations = annotationsChanged.ToArray();
            }


            // ClientType changes
            switch (contact.ClientType)
            {
                case IMAddressInfoType.WindowsLive:
                    {
                        // IsMessengerUser
                        if (abContactType.contactInfo.isMessengerUser != contact.IsMessengerUser)
                        {
                            propertiesChanged.Add(PropertyString.IsMessengerUser);
                            contactToChange.contactInfo.isMessengerUser = contact.IsMessengerUser;
                            contactToChange.contactInfo.isMessengerUserSpecified = true;
                            propertiesChanged.Add(PropertyString.MessengerMemberInfo); // Pang found WLM2009 add this.
                            contactToChange.contactInfo.MessengerMemberInfo = new MessengerMemberInfo(); // But forgot to add this...
                            contactToChange.contactInfo.MessengerMemberInfo.DisplayName = NSMessageHandler.Owner.Name; // and also this :)
                        }

                        // ContactType
                        if (abContactType.contactInfo.contactType != contact.ContactType)
                        {
                            propertiesChanged.Add(PropertyString.ContactType);
                            contactToChange.contactInfo.contactType = contact.ContactType;
                        }
                    }
                    break;

                case IMAddressInfoType.Yahoo:
                    {
                        if (abContactType.contactInfo.emails != null)
                        {
                            foreach (contactEmailType em in abContactType.contactInfo.emails)
                            {
                                if (em.email.ToLowerInvariant() == contact.Account.ToLowerInvariant() && em.isMessengerEnabled != contact.IsMessengerUser)
                                {
                                    propertiesChanged.Add(PropertyString.ContactEmail);
                                    contactToChange.contactInfo.emails = new contactEmailType[] { new contactEmailType() };
                                    contactToChange.contactInfo.emails[0].contactEmailType1 = ContactEmailTypeType.Messenger2;
                                    contactToChange.contactInfo.emails[0].isMessengerEnabled = contact.IsMessengerUser;
                                    contactToChange.contactInfo.emails[0].propertiesChanged = PropertyString.IsMessengerEnabled; //"IsMessengerEnabled";
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case IMAddressInfoType.Telephone:
                    {
                        if (abContactType.contactInfo.phones != null)
                        {
                            foreach (contactPhoneType ph in abContactType.contactInfo.phones)
                            {
                                if (ph.number == contact.Account && ph.isMessengerEnabled != contact.IsMessengerUser)
                                {
                                    propertiesChanged.Add(PropertyString.ContactPhone);
                                    contactToChange.contactInfo.phones = new contactPhoneType[] { new contactPhoneType() };
                                    contactToChange.contactInfo.phones[0].contactPhoneType1 = ContactPhoneTypes.ContactPhoneMobile;
                                    contactToChange.contactInfo.phones[0].isMessengerEnabled = contact.IsMessengerUser;
                                    contactToChange.contactInfo.phones[0].propertiesChanged = PropertyString.IsMessengerEnabled; //"IsMessengerEnabled";
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            if (propertiesChanged.Count > 0)
            {
                contactToChange.propertiesChanged = String.Join(PropertyString.propertySeparator, propertiesChanged.ToArray());
                UpdateContact(contactToChange, WebServiceConstants.MessengerIndividualAddressBookId, onSuccess);
            }
        }

        private void UpdateContact(ContactType contact, string abId, ABContactUpdateCompletedEventHandler onSuccess)
        {
            UpdateContactAsync(contact, abId,
                delegate(object service, ABContactUpdateCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        abRequest(PartnerScenario.ContactSave, delegate
                        {
                            if (onSuccess != null)
                                onSuccess(service, e);
                        });
                    }
                });
        }

        #endregion

        #region AddContactGroup & RemoveContactGroup & RenameGroup

        /// <summary>
        /// Send a request to the server to add a new contactgroup.
        /// </summary>
        /// <param name="groupName">The name of the group to add</param>
        public void AddContactGroup(string groupName)
        {
            ABGroupAddAsync(groupName,
                delegate(object service, ABGroupAddCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        NSMessageHandler.ContactGroups.AddGroup(new ContactGroup(groupName, e.Result.ABGroupAddResult.guid, NSMessageHandler, false));
                        NSMessageHandler.ContactService.OnContactGroupAdded(new ContactGroupEventArgs((ContactGroup)NSMessageHandler.ContactGroups[e.Result.ABGroupAddResult.guid]));
                    }
                });
        }

        /// <summary>
        /// Send a request to the server to remove a contactgroup. Any contacts in the group will also be removed from the forward list.
        /// </summary>
        /// <param name="contactGroup">The group to remove</param>
        public void RemoveContactGroup(ContactGroup contactGroup)
        {
            foreach (Contact cnt in NSMessageHandler.ContactList.All)
            {
                if (cnt.ContactGroups.Contains(contactGroup))
                {
                    throw new InvalidOperationException("Target group not empty, please remove all contacts form the group first.");
                }
            }

            ABGroupDeleteAsync(contactGroup,
                delegate(object service, ABGroupDeleteCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        NSMessageHandler.ContactGroups.RemoveGroup(contactGroup);
                        AddressBook.Groups.Remove(new Guid(contactGroup.Guid));
                        NSMessageHandler.ContactService.OnContactGroupRemoved(new ContactGroupEventArgs(contactGroup));
                    }
                });
        }


        /// <summary>
        /// Set the name of a contact group
        /// </summary>
        /// <param name="group">The contactgroup which name will be set</param>
        /// <param name="newGroupName">The new name</param>
        public void RenameGroup(ContactGroup group, string newGroupName)
        {
            ABGroupUpdateAsync(group, newGroupName,
                delegate(object service, ABGroupUpdateCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        group.SetName(newGroupName);
                    }
                });
        }

        #endregion

        #region AddContactToGroup & RemoveContactFromGroup

        public void AddContactToFavoriteGroup(Contact contact)
        {
            if (contact.Guid == Guid.Empty)
                throw new InvalidOperationException("This is not a valid Messenger contact.");

            ContactGroup favGroup = NSMessageHandler.ContactGroups.FavoriteGroup;

            if (favGroup != null && contact.HasGroup(favGroup) == false)
                AddContactToGroup(contact, favGroup);
            else
                throw new InvalidOperationException("No favorite group");
        }

        public void RemoveContactFromFavoriteGroup(Contact contact)
        {
            if (contact.Guid == Guid.Empty)
                throw new InvalidOperationException("This is not a valid Messenger contact.");

            ContactGroup favGroup = NSMessageHandler.ContactGroups.FavoriteGroup;

            if (favGroup != null && contact.HasGroup(favGroup))
                RemoveContactFromGroup(contact, favGroup);
            else
                throw new InvalidOperationException("No favorite group");
        }

        public void AddContactToGroup(Contact contact, ContactGroup group)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABGroupContactAdd", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            if (contact.Guid == Guid.Empty)
                throw new InvalidOperationException("This is not a valid Messenger contact.");

            ABGroupContactAddAsync(contact, group,
                delegate(object service, ABGroupContactAddCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        contact.AddContactToGroup(group);
                    }
                });
        }


        public void RemoveContactFromGroup(Contact contact, ContactGroup group)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("ABGroupContactDelete", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            if (contact.Guid == Guid.Empty)
                throw new InvalidOperationException("This is not a valid Messenger contact.");

            ABGroupContactDeleteAsync(contact, group,
                delegate(object service, ABGroupContactDeleteCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        contact.RemoveContactFromGroup(group);
                    }
                });
        }

        #endregion

        #region AddContactToList

        /// <summary>
        /// Send a request to the server to add this contact to a specific list.
        /// </summary>
        /// <param name="contact">The affected contact</param>
        /// <param name="serviceName">Service name (e.g. Messenger)</param>
        /// <param name="list">The list to place the contact in</param>
        /// <param name="onSuccess"></param>
        internal void AddContactToList(Contact contact, ServiceName serviceName, RoleLists list, EventHandler onSuccess)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("AddMember", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            if (list == RoleLists.Pending) //this causes disconnect 
                return;

            // check whether the update is necessary
            if (contact.HasLists(list))
                return;

            string payload = ContactList.GenerateMailListForAdl(contact, list, false);

            if (list == RoleLists.Forward)
            {
                NSMessageHandler.MessageProcessor.SendMessage(new NSPayLoadMessage("ADL", payload));
                contact.AddToList(list);

                if (onSuccess != null)
                {
                    onSuccess(this, EventArgs.Empty);
                }

                return;
            }

            AddMemberAsync(contact, serviceName, list,
                delegate(object service, AddMemberCompletedEventArgs e)
                {
                    if (null != e.Error && false == e.Error.Message.Contains("Member already exists"))
                    {
                        return;
                    }

                    contact.AddToList(list);
                    NSMessageHandler.ContactService.OnContactAdded(new ListMutateEventArgs(contact, list));

                    if (((list & RoleLists.Allow) == RoleLists.Allow) || ((list & RoleLists.Hide) == RoleLists.Hide))
                    {
                        NSMessageHandler.MessageProcessor.SendMessage(new NSPayLoadMessage("ADL", payload));
                    }

                    if (onSuccess != null)
                    {
                        onSuccess(this, EventArgs.Empty);
                    }

                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "AddMember completed: " + list, GetType().Name);

                });
        }

        #endregion

        #region RemoveContactFromList

        /// <summary>
        /// Send a request to the server to remove a contact from a specific list.
        /// </summary> 
        /// <param name="contact">The affected contact</param>
        /// <param name="serviceName">Service name</param>
        /// <param name="list">The list to remove the contact from</param>
        /// <param name="onSuccess"></param>
        internal void RemoveContactFromList(Contact contact, ServiceName serviceName, RoleLists list, EventHandler onSuccess)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("DeleteMember", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            // check whether the update is necessary
            if (!contact.HasLists(list))
                return;

            string payload = ContactList.GenerateMailListForAdl(contact, list, false);

            if (list == RoleLists.Forward)
            {
                NSMessageHandler.MessageProcessor.SendMessage(new NSPayLoadMessage("RML", payload));
                contact.RemoveFromList(list);

                if (onSuccess != null)
                {
                    onSuccess(this, EventArgs.Empty);
                }
                return;
            }

            DeleteMemberAsync(contact, serviceName, list,
                delegate(object service, DeleteMemberCompletedEventArgs e)
                {
                    if (null != e.Error && false == e.Error.Message.Contains("Member does not exist"))
                    {
                        return;
                    }

                    contact.RemoveFromList(list);
                    NSMessageHandler.ContactService.OnContactRemoved(new ListMutateEventArgs(contact, list));

                    if (((list & RoleLists.Allow) == RoleLists.Allow) || ((list & RoleLists.Hide) == RoleLists.Hide))
                    {
                        NSMessageHandler.MessageProcessor.SendMessage(new NSPayLoadMessage("RML", payload));
                    }

                    if (onSuccess != null)
                    {
                        onSuccess(this, EventArgs.Empty);
                    }

                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "DeleteMember completed: " + list, GetType().Name);

                });
        }

        #endregion

        #region Create Circle

        /// <summary>
        /// Use specific name to create a new <see cref="Contact"/>. <see cref="CreateCircleCompleted"/> event will be fired after creation succeeded.
        /// </summary>
        /// <param name="circleName">New circle name.</param>
        public void CreateCircle(string circleName)
        {
            if (NSMessageHandler.MSNTicket == MSNTicket.Empty || AddressBook == null)
            {
                OnServiceOperationFailed(this, new ServiceOperationFailedEventArgs("CreateCircle", new MSNPSharpException("You don't have access right on this action anymore.")));
                return;
            }

            CreateCircleAsync(circleName,
                delegate(object sender, CreateCircleCompletedEventArgs e)
                {
                    if (e.Error == null)
                    {
                        abRequest(PartnerScenario.JoinedCircleDuringPush,
                            delegate
                            {
                                lock (AddressBook.PendingCreateCircleList)
                                {
                                    AddressBook.PendingCreateCircleList[new Guid(e.Result.CreateCircleResult.Id)] = circleName;
                                }
                            }
                         );
                    }
                });
        }

        #endregion

        #region Invite/Reject/Accept/Leave circle

        /// <summary>
        /// Send and invitition to a specific contact to invite it join a <see cref="Contact"/>.
        /// </summary>
        /// <param name="circle">Circle to join.</param>
        /// <param name="contact">Contact being invited.</param>
        public void InviteCircleMember(Contact circle, Contact contact)
        {
            InviteCircleMember(circle, contact, string.Empty);
        }

        /// <summary>
        /// Send and invitition to a specific contact to invite it join a <see cref="Contact"/>. A message will send with the invitition.
        /// </summary>
        /// <param name="circle">Circle to join.</param>
        /// <param name="contact">Contact being invited.</param>
        /// <param name="message">Message send with the invitition email.</param>
        /// <exception cref="ArgumentNullException">One or more parameter(s) is/are null.</exception>
        /// <exception cref="InvalidOperationException">The owner is not the circle admin or the circle is blocked.</exception>
        public void InviteCircleMember(Contact circle, Contact contact, string message)
        {
            if (circle == null || contact == null || message == null)
            {
                throw new ArgumentNullException();
            }

            if (circle.AppearOffline)
            {
                throw new InvalidOperationException("Circle is on your block list.");
            }

            if (circle.CircleRole != RoleId.Admin &&
                circle.CircleRole != RoleId.AssistantAdmin)
            {
                throw new InvalidOperationException("The owner is not the administrator of this circle.");
            }

            if (contact == NSMessageHandler.Owner)
                return;

            CreateContactAsync(contact.Account, circle.ClientType, circle.AddressBookId,
                delegate(object sender, CreateContactCompletedEventArgs createContactCompletedArg)
                {
                    if (createContactCompletedArg.Error != null)
                        return;

                    ManageWLConnectionAsync(
                        new Guid(createContactCompletedArg.Result.CreateContactResult.contactId),
                        circle.AddressBookId,
                        message,
                        true,
                        false,
                        1,
                        RelationshipTypes.CircleGroup,
                        3 /*member*/,
                        delegate(object wlcSender, ManageWLConnectionCompletedEventArgs e)
                        {
                            if (e.Error != null)
                                return;

                            if (e.Result.ManageWLConnectionResult.contactInfo.clientErrorData != null &&
                                e.Result.ManageWLConnectionResult.contactInfo.clientErrorData != string.Empty)
                            {
                                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Invite circle member encounted a servier side error: " +
                                    e.Result.ManageWLConnectionResult.contactInfo.clientErrorData);
                            }

                            OnInviteCircleMemberCompleted(new CircleMemberEventArgs(circle, contact));
                        });
                }
            );
        }

        /// <summary>
        /// Reject a join circle invitation.
        /// </summary>
        /// <param name="circle">Circle to  join.</param>
        public void RejectCircleInvitation(Contact circle)
        {
            if (circle == null)
                throw new ArgumentNullException("circle");

            ManageWLConnectionAsync(circle.Guid, Guid.Empty, String.Empty, true, false, 2,
                RelationshipTypes.CircleGroup, 0,
                delegate(object wlcSender, ManageWLConnectionCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "[RejectCircleInvitation] Reject circle invitation failed: " + circle.ToString());
                    }
                });
        }


        internal void ServerNotificationRequest(string scene, object[] parameters, ABFindContactsPagedCompletedEventHandler onSuccess)
        {
            Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Executing notify addressbook request, PartnerScenario: " + scene);

            switch (scene)
            {
                case PartnerScenario.ABChangeNotifyAlert:
                    msRequest(scene,
                        delegate
                        {
                            ABNotifyChangedSaveReuqest(scene, onSuccess);
                        }
                    );
                    break;

                case PartnerScenario.CircleIdAlert:
                    abHandleType abHandler = new abHandleType();
                    abHandler.Puid = 0;
                    abHandler.Cid = 0;
                    abHandler.ABId = parameters[0].ToString();

                    msRequest(PartnerScenario.MessengerPendingList,
                        delegate
                        {
                            ABNotifyChangedSaveReuqest(scene, onSuccess);
                        }
                    );
                    break;
            }

        }

        private void ABNotifyChangedSaveReuqest(string scene, ABFindContactsPagedCompletedEventHandler onSuccess)
        {
            abRequest(scene,
                      delegate(object sender, ABFindContactsPagedCompletedEventArgs e)
                      {
                          if (e.Cancelled || e.Error != null)
                          {
                              return;
                          }

                          if (e.Result.ABFindContactsPagedResult.Contacts == null
                              && e.Result.ABFindContactsPagedResult.Groups == null)
                          {
                              if (e.Result.ABFindContactsPagedResult.CircleResult == null
                                  || e.Result.ABFindContactsPagedResult.CircleResult.Circles == null)
                              {
                                  return;
                              }
                          }

                          AddressBook.Save();

                          if (onSuccess != null)
                              onSuccess(sender, e);
                      }
                      );
        }

        /// <summary>
        /// Accept the circle invitation.
        /// </summary>
        /// <param name="circle"></param>
        /// <exception cref="ArgumentNullException">The circle parameter is null.</exception>
        /// <exception cref="InvalidOperationException">The circle specified is not a pending circle.</exception>
        public void AcceptCircleInvitation(Contact circle)
        {
            if (circle == null)
                throw new ArgumentNullException("circle");

            if (circle.CircleRole != RoleId.StatePendingOutbound)
                throw new InvalidOperationException("This is not a pending circle.");

            ManageWLConnectionAsync(circle.Guid, Guid.Empty, String.Empty, true, false, 1,
                RelationshipTypes.CircleGroup, 0,
                delegate(object wlcSender, ManageWLConnectionCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        return;
                    }

                    abRequest(PartnerScenario.JoinedCircleDuringPush, null);
                });
        }

        /// <summary>
        /// Leave the specific circle.
        /// </summary>
        /// <param name="circle"></param>
        /// <exception cref="ArgumentNullException">The circle parameter is null.</exception>
        public void ExitCircle(Contact circle)
        {
            if (circle == null)
                throw new ArgumentNullException("circle");

            Guid selfGuid = AddressBook.SelectSelfContactGuid(circle.AddressBookId.ToString("D"));
            Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Circle contactId: " + selfGuid);

            BreakConnectionAsync(selfGuid, circle.AddressBookId, false, true,
                delegate(object sender, BreakConnectionCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Exit circle: " + circle.AddressBookId.ToString("D") + " failed, error: " + e.Error.Message);
                        return;
                    }

                    NSMessageHandler.SendCircleNotifyRML(circle.AddressBookId, circle.HostDomain, circle.Lists);
                    AddressBook.RemoveCircle(circle.AddressBookId.ToString("D").ToLowerInvariant(), true);
                    AddressBook.Save();
                });
        }

        #endregion

        #endregion

        public override void Clear()
        {
            binarySemaphore.WaitOne();
            {
                base.Clear();

                // Last save for contact list files
                if (NSMessageHandler.IsSignedIn && AddressBook != null && Deltas != null)
                {
                    try
                    {
                        AddressBook.Save();
                        Deltas.Truncate();
                    }
                    catch (Exception error)
                    {
                        Trace.WriteLineIf(Settings.TraceSwitch.TraceError, error.Message, GetType().Name);
                    }
                }

                AddressBook = null;
                Deltas = null;
                abSynchronized = false;
            }
            binarySemaphore.Release();
        }

        #region DeleteRecordFile

        /// <summary>
        /// Delete the record file that contains the contactlist of owner.
        /// </summary>
        public void DeleteRecordFile()
        {
            DeleteRecordFile(false);
        }

        private void DeleteRecordFile(bool reCreate)
        {
            if (NSMessageHandler.Owner != null && NSMessageHandler.Owner.Account != null)
            {
                MclSerialization st = Settings.SerializationType;
                string addressbookFile = Path.Combine(Settings.SavePath, NSMessageHandler.Owner.Account.GetHashCode() + ".mcl");
                string deltasResultFile = Path.Combine(Settings.SavePath, NSMessageHandler.Owner.Account.GetHashCode() + "d" + ".mcl");

                // Re-init addressbook
                {
                    MclFile.Delete(addressbookFile, true);

                    if (reCreate)
                    {
                        AddressBook = XMLContactList.LoadFromFile(addressbookFile, st, NSMessageHandler, false);
                        AddressBook.Save();
                    }
                }

                //If we saved cachekey and preferred host in it, deltas can't be deleted.
                if (Deltas != null && reCreate)
                {
                    Deltas.Truncate();
                }
                else
                {
                    MclFile.Delete(deltasResultFile, true);

                    if (reCreate)
                    {
                        Deltas = DeltasList.LoadFromFile(deltasResultFile, st, NSMessageHandler.Credentials.Password, true);
                        Deltas.Save(true);
                    }
                }

                abSynchronized = false;
            }
        }

        #endregion
    }
};
