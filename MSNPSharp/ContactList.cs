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
using System.Xml;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace MSNPSharp
{
    using MSNPSharp.Core;
    using MSNPSharp.MSNWS.MSNABSharingService;

    [Serializable]
    public class ContactList : Dictionary<IMAddressInfoType, Dictionary<string, Contact>>
    {
        private static IMAddressInfoType[] addressTypes = (IMAddressInfoType[])Enum.GetValues(typeof(IMAddressInfoType));

        [NonSerialized]
        private NSMessageHandler nsMessageHandler;
        [NonSerialized]
        private object syncRoot;

        private Guid addressBookId = Guid.Empty;
        private Owner owner = null;
        private Contact gateway;

        public ContactList(NSMessageHandler handler)
            : this(WebServiceConstants.MessengerIndividualAddressBookId, null, handler)
        {
        }

        public ContactList(string abId, Owner owner, NSMessageHandler handler)
            : this(new Guid(abId), owner, handler)
        {
        }

        public ContactList(Guid abId, Owner owner, NSMessageHandler handler)
            : this(abId, owner, null, handler)
        {
        }

        public ContactList(Guid abId, Owner owner, Contact gateway, NSMessageHandler handler)
        {
            Reset();

            this.gateway = gateway;
            this.addressBookId = abId;
            this.nsMessageHandler = handler;
            this.owner = owner;
        }

        #region ListEnumerators

        public class ListEnumerator : IEnumerator<Contact>
        {
            protected Dictionary<string, Contact>.Enumerator baseEnum;
            protected RoleLists listFilter;

            public ListEnumerator(Dictionary<string, Contact>.Enumerator listEnum, RoleLists filter)
            {
                baseEnum = listEnum;
                listFilter = filter;
            }

            public virtual bool MoveNext()
            {
                while (baseEnum.MoveNext())
                {
                    if (Current.ContactType != MessengerContactType.Circle)
                    {
                        if (listFilter == RoleLists.None)
                        {
                            return true;
                        }

                        if (Current.HasLists(listFilter))
                            return true;
                    }
                }

                return false;
            }

            object IEnumerator.Current
            {
                get
                {
                    return baseEnum.Current;
                }
            }

            public Contact Current
            {
                get
                {
                    return baseEnum.Current.Value;
                }
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
                baseEnum.Dispose();
            }

            public IEnumerator<Contact> GetEnumerator()
            {
                return this;
            }

            public ContactList.ListEnumerator FilterByRoles(RoleLists roles)
            {
                return new ContactList.ListEnumerator(baseEnum, roles);
            }
        }

        public class EmailListEnumerator : ContactList.ListEnumerator
        {
            public EmailListEnumerator(Dictionary<string, Contact>.Enumerator listEnum)
                : base(listEnum, RoleLists.None)
            {
            }

            public override bool MoveNext()
            {
                while (base.MoveNext())
                {
                    if (Current.Guid != Guid.Empty && Current.IsMessengerUser == false)
                        return true;
                }
                return false;
            }
        }

        public class CircleListEnumerator : ContactList.ListEnumerator
        {
            public CircleListEnumerator(Dictionary<string, Contact>.Enumerator listEnum, RoleLists filter)
                : base(listEnum, filter)
            {
            }

            public override bool MoveNext()
            {
                while (baseEnum.MoveNext())
                {
                    if (Current.ContactType == MessengerContactType.Circle)
                    {
                        if (listFilter == RoleLists.None)
                        {
                            return true;
                        }

                        if (Current.HasLists(listFilter))
                            return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region Lists

        /// <summary>
        /// All contacts including all roles.
        /// </summary>
        public ContactList.ListEnumerator All
        {
            get
            {
                return new ContactList.ListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.None]).GetEnumerator(), RoleLists.None);
            }
        }

        /// <summary>
        /// All contacts on your address book.
        /// </summary>
        public ContactList.ListEnumerator Forward
        {
            get
            {
                return new ContactList.ListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.None]).GetEnumerator(), RoleLists.Forward);
            }
        }

        /// <summary>
        /// All contacts on your allowed list who can send instant messages.
        /// </summary>
        public ContactList.ListEnumerator Allowed
        {
            get
            {
                return new ContactList.ListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.None]).GetEnumerator(), RoleLists.Allow);
            }
        }

        /// <summary>
        /// All contacts on your hidden list who CANNOT see your status but CAN send offline messages.
        /// </summary>
        public ContactList.ListEnumerator Hidden
        {
            get
            {
                return new ContactList.ListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.None]).GetEnumerator(), RoleLists.Hide);
            }
        }

        /// <summary>
        ///  All pending contacts.
        /// </summary>
        public ContactList.ListEnumerator Pending
        {
            get
            {
                return new ContactList.ListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.None]).GetEnumerator(), RoleLists.Pending);
            }
        }

        /// <summary>
        /// All contacts on your email list. IsMessengerUser property is false.
        /// </summary>
        public ContactList.ListEnumerator Email
        {
            get
            {
                return new ContactList.EmailListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.None]).GetEnumerator());
            }
        }

        /// <summary>
        /// All msn groups (not categories) on your contact list introduces with WLM2009.
        /// </summary>
        public ContactList.ListEnumerator Circles
        {
            get
            {
                return new ContactList.CircleListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.Circle]).GetEnumerator(), RoleLists.None);
            }
        }

        /// <summary>
        /// All external networks like facebook.
        /// </summary>
        public ContactList.ListEnumerator ExternalNetworks
        {
            get
            {
                return new ContactList.ListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.RemoteNetwork]).GetEnumerator(), RoleLists.None);
            }
        }

        /// <summary>
        /// Facebook contacts.
        /// </summary>
        public ContactList.ListEnumerator Facebook
        {
            get
            {
                Contact fbNetwork = GetContact(RemoteNetworkGateways.FaceBookGatewayAccount, IMAddressInfoType.RemoteNetwork);

                if (fbNetwork != null && fbNetwork.ContactList != null)
                    return fbNetwork.ContactList.All;

                return null;
            }
        }

        public ContactList.ListEnumerator WindowsLive
        {
            get
            {
                return new ContactList.ListEnumerator(new Dictionary<string, Contact>(base[IMAddressInfoType.WindowsLive]).GetEnumerator(), RoleLists.None);
            }
        }

        #endregion

        #region Properties

        public object SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    Interlocked.CompareExchange(ref syncRoot, new object(), null);
                }
                return syncRoot;
            }
        }

        /// <summary>
        /// The addressbook identifier of this addressbook.
        /// </summary>
        public Guid AddressBookId
        {
            get
            {
                return addressBookId;
            }
        }

        /// <summary>
        /// The owner of the contactlist. This is the identity that logged into the messenger network.
        /// </summary>
        public Owner Owner
        {
            get
            {
                return owner;
            }
        }

        public Contact this[string account]
        {
            get
            {
                return GetContact(account);
            }
            set
            {
                this[account, value.ClientType] = value;
            }
        }

        public Contact this[string account, IMAddressInfoType type]
        {
            get
            {
                return GetContact(account, type);
            }
            set
            {
                IMAddressInfoType key = value.ClientType;

                if (type != IMAddressInfoType.None)
                {
                    Contact c = GetContactWithCreate(account, type);

                    if (!Object.ReferenceEquals(c, value))
                    {
                        string hash = Contact.MakeHash(account, type);

                        lock (SyncRoot)
                        {
                            base[key][hash] = value;
                            base[IMAddressInfoType.None][hash] = value;
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the specified contact.
        /// <remarks>If the contact does not exist, return null</remarks>
        /// </summary>
        /// <returns>
        /// If the contact does not exist, returns null.
        /// </returns>
        public Contact GetContact(string account)
        {
            foreach (IMAddressInfoType addressType in addressTypes)
            {
                if (addressType != IMAddressInfoType.None)
                {
                    lock (SyncRoot)
                    {
                        if (HasContact(account, addressType))
                            return GetContact(account, addressType);
                    }
                }
            }

            return null;
        }

        public Contact GetContact(string account, IMAddressInfoType addressType)
        {
            if (addressType != IMAddressInfoType.None)
            {
                string hash = Contact.MakeHash(account, addressType);
                lock (SyncRoot)
                {
                    if (base[addressType].ContainsKey(hash))
                    {
                        return base[addressType][hash];
                    }
                }
            }
            return null;
        }

        public Contact GetCircle(string account)
        {
            lock (SyncRoot)
            {
                if (HasContact(account, IMAddressInfoType.Circle))
                    return GetContact(account, IMAddressInfoType.Circle);

                return null;
            }
        }

        public Contact GetContactByGuid(Guid guid)
        {
            if (guid != Guid.Empty)
            {
                lock (SyncRoot)
                {
                    foreach (Contact contact in base[IMAddressInfoType.None].Values)
                    {
                        if (contact.Guid == guid)
                            return contact;
                    }
                }
            }
            return null;
        }

        public Contact GetContactByCID(long cid)
        {
            if (cid != 0)
            {
                lock (SyncRoot)
                {
                    foreach (Contact contact in base[IMAddressInfoType.None].Values)
                    {
                        if (contact.CID == cid)
                            return contact;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Check whether the specified account is in the contact list.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool HasContact(string account)
        {
            foreach (IMAddressInfoType ct in addressTypes)
            {
                if (ct != IMAddressInfoType.None && HasContact(account, ct))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check whether the account with specified client type is in the contact list.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="addressType"></param>
        /// <returns></returns>
        public bool HasContact(string account, IMAddressInfoType addressType)
        {
            if (addressType != IMAddressInfoType.None)
            {
                string hash = Contact.MakeHash(account, addressType);
                lock (SyncRoot)
                    return base[addressType].ContainsKey(hash);
            }
            return false;
        }

        public bool HasMultiType(string account)
        {
            int typecount = 0;
            foreach (IMAddressInfoType ct in addressTypes)
            {
                if (HasContact(account, ct) && ++typecount > 1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a contact with specified account and client type.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="type"></param>
        public bool Remove(string account, IMAddressInfoType type)
        {
            bool removed = false;

            if (type != IMAddressInfoType.None)
            {
                string hash = Contact.MakeHash(account, type);

                lock (SyncRoot)
                {
                    removed = base[type].Remove(hash);

                    if (removed)
                    {
                        bool found = false;

                        foreach (IMAddressInfoType deleteList in addressTypes)
                        {
                            if (deleteList != IMAddressInfoType.None)
                            {
                                if (base[deleteList].ContainsKey(hash))
                                {
                                    found = true; // Can't be deleted...
                                    break;
                                }
                            }
                        }

                        // None found. It is time to remove from none list, too.
                        if (!found)
                        {
                            base[IMAddressInfoType.None].Remove(hash);
                        }
                    }
                }
            }

            return removed;
        }

        /// <summary>
        /// Reset the contact list and clear the owner.
        /// </summary>
        public void Reset()
        {
            lock (SyncRoot)
            {
                if (Owner != null)
                {
                    Owner.Emoticons.Clear();
                    Owner.EndPointData.Clear();
                    Owner.LocalEndPointIMCapabilities = ClientCapabilities.None;
                    Owner.LocalEndPointIMCapabilitiesEx = ClientCapabilitiesEx.None;
                    Owner.LocalEndPointPECapabilities = ClientCapabilities.None;
                    Owner.LocalEndPointPECapabilitiesEx = ClientCapabilitiesEx.None;
                }

                owner = null;


                foreach (IMAddressInfoType addressType in addressTypes)
                {
                    base[addressType] = new Dictionary<string, Contact>();
                }
            }
        }

        /// <summary>
        /// Copy the whole contact list out.
        /// </summary>
        /// <returns></returns>
        public Contact[] ToArray(IMAddressInfoType type)
        {
            lock (SyncRoot)
            {
                Contact[] array = new Contact[base[type].Values.Count];
                base[type].Values.CopyTo(array, 0);
                return array;
            }
        }

        public static RoleId GetMemberRole(RoleLists list)
        {
            switch (list)
            {
                case RoleLists.Allow:
                    return RoleId.Allow;

                case RoleLists.Pending:
                    return RoleId.Pending;

                case RoleLists.Hide:
                    return RoleId.Hide;
            }
            return RoleId.None;
        }

        public static RoleLists GetMSNList(RoleId memberRole)
        {
            switch (memberRole)
            {
                case RoleId.Allow:
                    return RoleLists.Allow;
                case RoleId.Pending:
                    return RoleLists.Pending;
                case RoleId.Hide:
                    return RoleLists.Hide;
            }
            return RoleLists.None;
        }

        public static string GenerateMailListForAdl(Contact contact, RoleLists lists, bool initial)
        {
            Dictionary<string, RoleLists> hashlist = new Dictionary<string, RoleLists>(1);
            hashlist.Add(contact.Hash, lists);
            return GenerateMailListForAdl(hashlist, initial)[0];
        }

        public static string[] GenerateMailListForAdl(Dictionary<string, RoleLists> contacts, bool initial)
        {
            List<string> mls = new List<string>();
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement mlElement = xmlDoc.CreateElement("ml");
            if (initial)
                mlElement.SetAttribute("l", "1");

            if (contacts == null || contacts.Count == 0)
            {
                mls.Add(mlElement.OuterXml);
                return mls.ToArray();
            }

            List<string> sortedContacts = new List<string>(contacts.Keys);
            sortedContacts.Sort(CompareContactsHash);

            int domaincontactcount = 0;
            string currentDomain = null;
            XmlElement domtelElement = null;

            foreach (string contact_hash in sortedContacts)
            {
                String name;
                String domain;
                string[] arr = contact_hash.Split(new string[] { ":", ";via=" }, StringSplitOptions.RemoveEmptyEntries);
                String type = IMAddressInfoType.Yahoo.ToString();
                if (arr.Length > 0)
                    type = arr[0];

                IMAddressInfoType clitype = (IMAddressInfoType)Enum.Parse(typeof(IMAddressInfoType), type);
                type = ((int)clitype).ToString();
                RoleLists imlist = contacts[contact_hash];
                RoleLists pelist = imlist;
                pelist &= ~RoleLists.Hide; // Remove Hide role from PE service

                if (clitype == IMAddressInfoType.Telephone)
                {
                    if (!arr[1].StartsWith("+"))
                        continue;

                    domain = String.Empty;
                    name = "tel:" + arr[1];
                }
                else if (clitype == IMAddressInfoType.RemoteNetwork)
                {
                    domain = String.Empty;
                    name = arr[1];
                }
                else
                {
                    String[] usernameanddomain = arr[1].Split('@');
                    domain = usernameanddomain[1];
                    name = usernameanddomain[0];
                }

                if (imlist != RoleLists.None)
                {
                    if (currentDomain != domain)
                    {
                        currentDomain = domain;
                        domaincontactcount = 0;

                        if (clitype == IMAddressInfoType.Telephone)
                        {
                            domtelElement = xmlDoc.CreateElement("t");
                        }
                        else if (clitype == IMAddressInfoType.RemoteNetwork)
                        {
                            domtelElement = xmlDoc.CreateElement("n");
                        }
                        else
                        {
                            domtelElement = xmlDoc.CreateElement("d");
                            domtelElement.SetAttribute("n", currentDomain);
                        }
                        mlElement.AppendChild(domtelElement);
                    }

                    XmlElement contactElement = xmlDoc.CreateElement("c");
                    contactElement.SetAttribute("n", name);

                    if (clitype != IMAddressInfoType.Telephone && clitype != IMAddressInfoType.RemoteNetwork)
                        contactElement.SetAttribute("t", type);

                    // IM
                    XmlElement IMservice = xmlDoc.CreateElement("s");
                    IMservice.SetAttribute("l", ((int)imlist).ToString());
                    IMservice.SetAttribute("n", ServiceShortNames.IM.ToString());
                    contactElement.AppendChild(IMservice);

                    // PE
                    if (pelist != RoleLists.None)
                    {
                        XmlElement PEservice = xmlDoc.CreateElement("s");
                        PEservice.SetAttribute("l", ((int)pelist).ToString());
                        PEservice.SetAttribute("n", ServiceShortNames.PE.ToString());
                        contactElement.AppendChild(PEservice);
                    }

                    domtelElement.AppendChild(contactElement);
                    domaincontactcount++;

                }

                if (mlElement.OuterXml.Length > 7300)
                {
                    mlElement.AppendChild(domtelElement);
                    mls.Add(mlElement.OuterXml);

                    mlElement = xmlDoc.CreateElement("ml");
                    if (initial)
                        mlElement.SetAttribute("l", "1");

                    currentDomain = null;
                    domaincontactcount = 0;
                }
            }

            if (domaincontactcount > 0 && domtelElement != null)
                mlElement.AppendChild(domtelElement);

            mls.Add(mlElement.OuterXml);
            return mls.ToArray();
        }


        private static int CompareContactsHash(string hash1, string hash2)
        {
            string[] str_arr1 = hash1.Split(new string[] { ":", ";via=" }, StringSplitOptions.RemoveEmptyEntries);
            string[] str_arr2 = hash2.Split(new string[] { ":", ";via=" }, StringSplitOptions.RemoveEmptyEntries);

            if (str_arr1.Length == 0)
                return 1;

            else if (str_arr2.Length == 0)
                return -1;

            string xContact, yContact;

            if (str_arr1[1].IndexOf("@") == -1)
                xContact = str_arr1[1];
            else
                xContact = str_arr1[1].Substring(str_arr1[1].IndexOf("@") + 1);

            if (str_arr2[1].IndexOf("@") == -1)
                yContact = str_arr2[1];
            else
                yContact = str_arr2[1].Substring(str_arr2[1].IndexOf("@") + 1);

            return String.Compare(xContact, yContact, true, CultureInfo.InvariantCulture);
        }



        #region Internal

        /// <summary>
        /// Get a contact with specified account and client type, if the contact does not exist, create it.
        /// <para>This overload will set the contact name to a specified value.</para>
        /// </summary>
        /// <param name="account"></param>
        /// <param name="name">The new name you want to set.</param>
        /// <param name="type"></param>
        /// <returns>
        /// A <see cref="Contact"/> object.
        /// If the contact does not exist, create it.
        /// </returns>
        internal Contact GetContact(string account, string name, IMAddressInfoType type)
        {
            Contact contact = GetContactWithCreate(account, type);

            lock (SyncRoot)
                contact.SetName(name);

            return contact;
        }

        /// <summary>
        /// Get a contact with specified account and client type, if the contact does not exist, create it.
        /// </summary>
        /// <param name="account">Account (Mail) of a contact</param>
        /// <param name="type">Contact type.</param>
        /// <returns>
        /// A <see cref="Contact"/> object.
        /// If the contact does not exist, create it.
        /// </returns>
        internal Contact GetContactWithCreate(string account, IMAddressInfoType type)
        {
            if (type == IMAddressInfoType.None)
                return null;

            string hash = Contact.MakeHash(account, type);

            if (base[type].ContainsKey(hash))
            {
                return base[type][hash];
            }

            Contact tmpContact = null;

            if (type == IMAddressInfoType.Circle)
            {
                string[] accountAndDomain = account.ToLowerInvariant().Split('@');
                Guid circleABID = new Guid(accountAndDomain[0]);
                tmpContact = new Contact(circleABID, account, type, 0, nsMessageHandler);
                tmpContact.ContactType = MessengerContactType.Circle;
                tmpContact.HostDomain = accountAndDomain[1];

                // Contact list created when contact.SetCircleInfo called.
            }
            else if (type == IMAddressInfoType.RemoteNetwork)
            {
                tmpContact = new Contact(AddressBookId, account, type, 0, nsMessageHandler);
                tmpContact.ContactList = new ContactList(AddressBookId, null, tmpContact, nsMessageHandler);
            }
            else
            {
                tmpContact = new Contact(AddressBookId, account, type, 0, nsMessageHandler);
                tmpContact.Via = this.gateway;
            }

            lock (SyncRoot)
            {
                base[type][hash] = tmpContact;
                base[IMAddressInfoType.None][hash] = tmpContact;
            }

            return GetContact(account, type);
        }

        /// <summary>
        /// Create the shell contact
        /// </summary>
        /// <param name="coreContact"></param>
        /// <param name="type"></param>
        /// <param name="objectID">The Contact Account</param>
        /// <returns></returns>
        internal ShellContact CreateShellContact(Contact coreContact, IMAddressInfoType type, string objectID)
        {
            ShellContact shellContact = (coreContact != null) ?
                new ShellContact(coreContact, type, objectID, nsMessageHandler)
                :
                new ShellContact(AddressBookId, objectID, type, nsMessageHandler);

            string hash = Contact.MakeHash(objectID, type);

            if (type == IMAddressInfoType.Connect)
                shellContact.Via = this.gateway; // sourceID

            lock (SyncRoot)
            {
                base[type][hash] = shellContact;
                base[IMAddressInfoType.None][hash] = shellContact;
            }

            return (ShellContact)GetContact(shellContact.Account, shellContact.ClientType);
        }

        internal ShellContact CreateShellContact(string objectID, IMAddressInfoType type)
        {
            return CreateShellContact(null, type, objectID);
        }

        /// <summary>
        /// Set the owner for default addressbook. This funcation can be only called once.
        /// </summary>
        /// <param name="owner"></param>
        internal void SetOwner(Owner owner)
        {
            if (AddressBookId != new Guid(WebServiceConstants.MessengerIndividualAddressBookId))
            {
                throw new InvalidOperationException("Only default addressbook can call this function.");
            }

            if (this.Owner != null)
            {
                throw new InvalidOperationException("Owner already set.");
            }

            if (owner.AddressBookId != new Guid(WebServiceConstants.MessengerIndividualAddressBookId))
            {
                throw new InvalidOperationException("Invalid owner: This is not the owner for default addressbook.");
            }

            this.owner = owner;
        }

        #endregion
    }
};