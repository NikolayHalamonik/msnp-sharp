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
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;

namespace MSNPSharp
{
    using MSNPSharp.IO;
    using MSNPSharp.Core;
    using System.Threading;
    using MSNPSharp.MSNWS.MSNStorageService;

    [Serializable]
    public class Owner : Contact
    {
        /// <summary>
        /// Fired when owner profile received.
        /// </summary>
        public event EventHandler<EventArgs> ProfileReceived;

        private string epName = Environment.MachineName;
        private bool passportVerified;

        public Owner(string abId, string account, long cid, NSMessageHandler handler)
            : this(new Guid(abId), account, cid, handler)
        {
        }

        public Owner(Guid abId, string account, long cid, NSMessageHandler handler)
            : base(abId, account, IMAddressInfoType.WindowsLive, cid, handler)
        {
        }

        internal void CreateDefaultDisplayImage(SerializableMemoryStream sms)
        {
            if (sms == null)
            {
                sms = new SerializableMemoryStream();
                Image msnpsharpDefaultImage = Properties.Resources.MSNPSharp_logo.Clone() as Image;
                msnpsharpDefaultImage.Save(sms, msnpsharpDefaultImage.RawFormat);
            }

            DisplayImage displayImage = new DisplayImage(Account.ToLowerInvariant(), sms);

            this.DisplayImage = displayImage;
        }

        /// <summary>
        /// This place's name
        /// </summary>
        public string EpName
        {
            get
            {
                return epName;
            }
            set
            {
                if (NSMessageHandler != null && NSMessageHandler.IsSignedIn && Status != PresenceStatus.Offline)
                {
                    NSMessageHandler.SetPresenceStatus(
                        Status,
                        LocalEndPointIMCapabilities, LocalEndPointIMCapabilitiesEx,
                        LocalEndPointPECapabilities, LocalEndPointPECapabilitiesEx,
                        value, PersonalMessage, false);
                }

                epName = value;
            }
        }

        /// <summary>
        /// Get or set the IM <see cref="ClientCapabilities"/> of local end point.
        /// </summary>
        public ClientCapabilities LocalEndPointIMCapabilities
        {
            get
            {
                if (EndPointData.ContainsKey(NSMessageHandler.MachineGuid))
                    return EndPointData[NSMessageHandler.MachineGuid].IMCapabilities;

                return ClientCapabilities.None;
            }
            set
            {
                if (value != LocalEndPointIMCapabilities)
                {
                    NSMessageHandler.SetPresenceStatus(
                        Status,
                        value, LocalEndPointIMCapabilitiesEx,
                        LocalEndPointPECapabilities, LocalEndPointPECapabilitiesEx,
                        EpName, PersonalMessage, false);

                    EndPointData[NSMessageHandler.MachineGuid].IMCapabilities = value;
                }
            }
        }

        /// <summary>
        /// Get or set the IM <see cref="ClientCapabilitiesEx"/> of local end point.
        /// </summary>
        public ClientCapabilitiesEx LocalEndPointIMCapabilitiesEx
        {
            get
            {
                if (EndPointData.ContainsKey(NSMessageHandler.MachineGuid))
                    return EndPointData[NSMessageHandler.MachineGuid].IMCapabilitiesEx;

                return ClientCapabilitiesEx.None;
            }

            set
            {
                if (value != LocalEndPointIMCapabilitiesEx)
                {
                    NSMessageHandler.SetPresenceStatus(
                        Status,
                        LocalEndPointIMCapabilities, value,
                        LocalEndPointPECapabilities, LocalEndPointPECapabilitiesEx,
                        EpName, PersonalMessage, false);

                    EndPointData[NSMessageHandler.MachineGuid].IMCapabilitiesEx = value;
                }
            }
        }

        /// <summary>
        /// Get or set the PE (P2P) <see cref="ClientCapabilities"/> of local end point.
        /// </summary>
        public ClientCapabilities LocalEndPointPECapabilities
        {
            get
            {
                if (EndPointData.ContainsKey(NSMessageHandler.MachineGuid))
                    return EndPointData[NSMessageHandler.MachineGuid].PECapabilities;

                return ClientCapabilities.None;
            }
            set
            {
                if (value != LocalEndPointPECapabilities)
                {
                    NSMessageHandler.SetPresenceStatus(
                        Status,
                        LocalEndPointIMCapabilities, LocalEndPointIMCapabilitiesEx,
                        value, LocalEndPointPECapabilitiesEx,
                        EpName, PersonalMessage, false);

                    EndPointData[NSMessageHandler.MachineGuid].PECapabilities = value;
                }
            }
        }

        /// <summary>
        /// Get or set the PE (P2P) <see cref="ClientCapabilitiesEx"/> of local end point.
        /// </summary>
        public ClientCapabilitiesEx LocalEndPointPECapabilitiesEx
        {
            get
            {
                if (EndPointData.ContainsKey(NSMessageHandler.MachineGuid))
                    return EndPointData[NSMessageHandler.MachineGuid].PECapabilitiesEx;

                return ClientCapabilitiesEx.None;
            }

            set
            {
                if (value != LocalEndPointPECapabilitiesEx)
                {
                    NSMessageHandler.SetPresenceStatus(
                        Status,
                        LocalEndPointIMCapabilities, LocalEndPointIMCapabilitiesEx,
                        LocalEndPointPECapabilities, value,
                        EpName, PersonalMessage, false);

                    EndPointData[NSMessageHandler.MachineGuid].PECapabilitiesEx = value;
                }
            }
        }

        /// <summary>
        /// Sign the owner out from every place.
        /// </summary>
        public void SignoutFromEverywhere()
        {
            foreach (Guid place in EndPointData.Keys)
            {
                if (place != NSMessageHandler.MachineGuid)
                {
                    SignoutFrom(place);
                }
            }

            SignoutFrom(NSMessageHandler.MachineGuid);
        }

        /// <summary>
        /// Sign the owner out from the specificed place.
        /// </summary>
        /// <param name="endPointID">The EndPoint guid to be signed out</param>
        public void SignoutFrom(Guid endPointID)
        {
            if (endPointID == Guid.Empty)
            {
                SignoutFromEverywhere();
                return;
            }

            if (EndPointData.ContainsKey(endPointID))
            {
                NSMessageHandler.SignoutFrom(endPointID);
            }
            else
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceWarning, "Invalid place (signed out already): " + endPointID.ToString("B"), GetType().Name);
            }
        }

        /// <summary>
        /// Owner display image. The image is broadcasted automatically.
        /// </summary>
        public override DisplayImage DisplayImage
        {
            get
            {
                return base.DisplayImage;
            }

            internal set
            {
                if (value != null)
                {
                    if (base.DisplayImage != null)
                    {
                        if (value.Sha == base.DisplayImage.Sha)
                        {
                            return;
                        }

                        MSNObjectCatalog.GetInstance().Remove(base.DisplayImage);
                    }

                    SetDisplayImageAndFireDisplayImageChangedEvent(value);
                    value.Creator = Account;

                    MSNObjectCatalog.GetInstance().Add(base.DisplayImage);

                    PersonalMessage pm = PersonalMessage;

                    pm.UserTileLocation = value.IsDefaultImage ? string.Empty : value.ContextPlain;

                    if (NSMessageHandler != null && NSMessageHandler.IsSignedIn &&
                        Status != PresenceStatus.Offline && Status != PresenceStatus.Unknown)
                    {
                        NSMessageHandler.SetPresenceStatus(
                            Status,
                            LocalEndPointIMCapabilities, LocalEndPointIMCapabilitiesEx,
                            LocalEndPointPECapabilities, LocalEndPointPECapabilitiesEx,
                            EpName, PersonalMessage, true);
                    }
                }
            }
        }

        public override SceneImage SceneImage
        {
            get
            {
                return base.SceneImage;
            }
            internal set
            {
                if (value != null)
                {
                    if (base.SceneImage != null)
                    {
                        if (value.Sha == base.SceneImage.Sha)
                        {
                            return;
                        }
                    }

                    value.Creator = Account;
                    base.SetSceneImage(value);
                }
            }
        }

        public override Color ColorScheme
        {
            get
            {
                return base.ColorScheme;
            }
            internal set
            {
                if (ColorScheme != value)
                {
                    base.ColorScheme = value;
                    NSMessageHandler.ContactService.Deltas.Profile.ColorScheme = ColorTranslator.ToOle(value);

                    base.OnColorSchemeChanged();
                }
            }
        }

        /// <summary>
        /// Set the scene image and the scheme color for the owner.
        /// </summary>
        /// <param name="imageScene">Set this to null or the default display image if you want the default MSN scene.</param>
        /// <param name="schemeColor"></param>
        /// <returns>
        /// The result will return false if the image scene and color are the same, compared to the current one.
        /// </returns>
        public bool SetScene(Image imageScene, Color schemeColor)
        {
            if (imageScene == SceneImage.Image && schemeColor == ColorScheme)
                return false;

            ColorScheme = schemeColor;
            if (imageScene != SceneImage.Image)
            {
                if (imageScene != null)
                {
                    MemoryStream sms = new MemoryStream();
                    imageScene.Save(sms, imageScene.RawFormat);

                    SceneImage = new SceneImage(NSMessageHandler.Owner.Account.ToLowerInvariant(), sms);
                }
                else
                    SceneImage = new SceneImage(NSMessageHandler.Owner.Account.ToLowerInvariant(), true);

                SaveOriginalSceneImageAndFireSceneImageChangedEvent(
                    new SceneImageChangedEventArgs(SceneImage, DisplayImageChangedType.TransmissionCompleted, false));
            }
            else
                NSMessageHandler.ContactService.Deltas.Save(true);

            if (NSMessageHandler != null)
                NSMessageHandler.SetSceneData(SceneImage, ColorScheme);

            return true;
        }

        /// <summary>
        /// Personel message
        /// </summary>
        public new PersonalMessage PersonalMessage
        {
            get
            {
                return base.PersonalMessage;
            }
            set
            {
                if (NSMessageHandler != null && NSMessageHandler.IsSignedIn)
                {
                    NSMessageHandler.SetPersonalMessage(value);
                }

                if (value != null)
                    base.PersonalMessage = value;
            }
        }

        public new string MobilePhone
        {
            get
            {
                return base.MobilePhone;
            }
            set
            {
                PhoneNumbers[ContactPhoneTypes.ContactPhoneMobile] = value;
            }
        }

        public new string WorkPhone
        {
            get
            {
                return base.WorkPhone;
            }
            set
            {
                if (NSMessageHandler != null)
                {
                    PhoneNumbers[ContactPhoneTypes.ContactPhoneBusiness] = value;
                }
            }
        }

        public new string HomePhone
        {
            get
            {
                return base.HomePhone;
            }
            set
            {
                if (NSMessageHandler != null)
                {
                    PhoneNumbers[ContactPhoneTypes.ContactPhonePersonal] = value;
                }
            }
        }

        public new bool MobileDevice
        {
            get
            {
                return base.MobileDevice;
            }
        }

        public new bool MobileAccess
        {
            get
            {
                return base.MobileAccess;
            }
        }

        /// <summary>
        /// Whether this account is verified by email. If an account is not verified, "(email not verified)" will be displayed after a contact's displayname.
        /// </summary>
        public bool PassportVerified
        {
            get
            {
                return passportVerified;
            }
            internal set
            {
                passportVerified = value;
            }
        }

        public override PresenceStatus Status
        {
            get
            {
                return base.Status;
            }

            set
            {
                if (NSMessageHandler != null)
                {
                    NSMessageHandler.SetPresenceStatus(
                        value,
                        LocalEndPointIMCapabilities, LocalEndPointIMCapabilitiesEx,
                        LocalEndPointPECapabilities, LocalEndPointPECapabilitiesEx,
                        EpName, PersonalMessage, false);
                }
            }
        }


        public override string Name
        {
            get
            {
                if (PersonalMessage != null)
                {
                    if (!string.IsNullOrEmpty(PersonalMessage.FriendlyName))
                    {
                        return PersonalMessage.FriendlyName;
                    }
                }

                return string.IsNullOrEmpty(base.Name) ? NickName : base.Name;
            }

            set
            {
                if (Name == value)
                    return;

                if (NSMessageHandler != null)
                {
                    NSMessageHandler.SetScreenName(value);
                }
            }
        }


        #region Profile datafields

        private Dictionary<string, string> msgProfile = new Dictionary<string, string>();
        bool validProfile;

        public bool ValidProfile
        {
            get
            {
                return validProfile;
            }
            internal set
            {
                validProfile = value;
            }
        }

        public bool EmailEnabled
        {
            get
            {
                return msgProfile.ContainsKey("EmailEnabled") && msgProfile["EmailEnabled"] == "1";
            }
            set
            {
                msgProfile["EmailEnabled"] = value ? "1" : "0";
            }
        }

        public long MemberIdHigh
        {
            get
            {
                return msgProfile.ContainsKey("MemberIdHigh") ? long.Parse(msgProfile["MemberIdHigh"]) : 0;
            }
            set
            {
                msgProfile["MemberIdHigh"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public long MemberIdLowd
        {
            get
            {
                return msgProfile.ContainsKey("MemberIdLow") ? long.Parse(msgProfile["MemberIdLow"]) : 0;
            }
            set
            {
                msgProfile["MemberIdLow"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public string PreferredLanguage
        {
            get
            {
                return msgProfile.ContainsKey("lang_preference") ? msgProfile["lang_preference"] : String.Empty;
            }
            set
            {
                msgProfile["lang_preference"] = value;
            }
        }

        public string Country
        {
            get
            {
                return msgProfile.ContainsKey("country") ? msgProfile["country"] : String.Empty;
            }
            set
            {
                msgProfile["country"] = value;
            }
        }

        public string Kid
        {
            get
            {
                return msgProfile.ContainsKey("Kid") ? msgProfile["Kid"] : String.Empty;
            }
            set
            {
                msgProfile["Kid"] = value;
            }
        }

        public long Flags
        {
            get
            {
                return msgProfile.ContainsKey("Flags") ? long.Parse(msgProfile["Flags"]) : 0;
            }
            set
            {
                msgProfile["Flags"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public string Sid
        {
            get
            {
                return msgProfile.ContainsKey("Sid") ? msgProfile["Sid"] : String.Empty;
            }
            set
            {
                msgProfile["Sid"] = value;
            }
        }

        public IPAddress ClientIP
        {
            get
            {
                return msgProfile.ContainsKey("ClientIP") ? IPAddress.Parse(msgProfile["ClientIP"]) : IPAddress.None;
            }
            set
            {
                msgProfile["ClientIP"] = value.ToString();
            }
        }

        /// <summary>
        /// Route address, used for PNRP??
        /// </summary>
        public string RouteInfo
        {
            get
            {
                return msgProfile.ContainsKey("RouteInfo") ? msgProfile["RouteInfo"] : String.Empty;
            }
            internal set
            {
                msgProfile["RouteInfo"] = value;
            }
        }

        public int ClientPort
        {
            get
            {
                return msgProfile.ContainsKey("ClientPort") ? int.Parse(msgProfile["ClientPort"]) : 0;
            }
            set
            {
                msgProfile["ClientPort"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        #endregion

        /// <summary>
        /// Save all the owner's profile to Widnows Live Services.
        /// </summary>
        /// <remarks>
        /// This function will not return until the update was completed. If you want to use a asynchronous
        /// version, please look at <see cref="UpdateRoamingProfileAsync"/>
        /// </remarks>
        public void UpdateRoamingProfile()
        {
            Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo, "Updating owner profile, please wait....");
            NSMessageHandler.StorageService.UpdateProfile(Name, PersonalMessage.Message);
            Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo, "Update owner profile completed.");
        }

        /// <summary>
        /// Save the display image of the owner to raoming profile.
        /// </summary>
        /// <param name="displayImageObject">The <see cref="Image"/> that casted to an <see cref="object"/></param>
        public void UpdateRoamingProfile(object displayImageObject)
        {
            Image displayImage = displayImageObject as Image;
            if (displayImage == null)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Cannot update display image, invalid object found.");
                return;
            }

            Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo, "Updating owner profile, please wait....");
            bool result = NSMessageHandler.StorageService.UpdateProfile(displayImage, "MyPhoto", false);
            Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo, "Update displayimage completed. Result = " + result);
        }

        public void UpdateRoamingProfileAsync()
        {
            Thread updateThread = new Thread(new ThreadStart(UpdateRoamingProfile));
            updateThread.Start();
        }

        public void UpdateRoamingProfileSync(Image displayImage)
        {
            Thread updateThread = new Thread(new ParameterizedThreadStart(UpdateRoamingProfile));
            updateThread.Start(displayImage);
        }

        public void UpdateDisplayImage(Image newImage)
        {
            if (newImage == null)
                return;
            MemoryStream imageStream = new MemoryStream();
            newImage.Save(imageStream, newImage.RawFormat);
            this.DisplayImage = new DisplayImage(this.Account.ToLowerInvariant(), imageStream);
        }

        /*
            EmailEnabled: 1
            MemberIdHigh: 123456
            MemberIdLow: -1234567890
            lang_preference: 2052
            country: US
            Kid: 0
            Flags: 1073742915
            sid: 72652
            ClientIP: XXX.XXX.XXX.XXX
            Nickname: New
            RouteInfo: msnp://XXX.XXX.XXX.XXX/013557A5
        */
        internal void UpdateProfile(StrDictionary hdr)
        {
            foreach (StrKeyValuePair pair in hdr)
            {
                msgProfile[String.Copy(pair.Key)] = String.Copy(pair.Value);
            }

            ValidProfile = true;

            if (msgProfile.ContainsKey("Nickname"))
                SetNickName(msgProfile["Nickname"]);

            OnProfileReceived(EventArgs.Empty);
        }

        internal void SyncProfileToDeltas()
        {

            if (CoreProfile.ContainsKey(CoreProfileAttributeName.PublicProfile_ResourceId))
            {
                if (NSMessageHandler.ContactService.Deltas.Profile == null)
                {
                    NSMessageHandler.ContactService.Deltas.Profile = new OwnerProfile();
                }

                NSMessageHandler.ContactService.Deltas.Profile.ResourceID = CoreProfile[CoreProfileAttributeName.PublicProfile_ResourceId].ToString();

                if (CoreProfile.ContainsKey(CoreProfileAttributeName.LastModified))
                    NSMessageHandler.ContactService.Deltas.Profile.DateModified = CoreProfile[CoreProfileAttributeName.LastModified].ToString();

            }


            if (CoreProfile.ContainsKey(CoreProfileAttributeName.PictureProfile_UserTileStatic_ResourceId))
            {
                NSMessageHandler.ContactService.Deltas.Profile.Photo.ResourceID = CoreProfile[CoreProfileAttributeName.PictureProfile_UserTileStatic_ResourceId].ToString();
            }

            if (CoreProfile.ContainsKey(CoreProfileAttributeName.UserTileStaticUrl))
            {
                NSMessageHandler.ContactService.Deltas.Profile.Photo.PreAthURL = CoreProfile[CoreProfileAttributeName.UserTileStaticUrl].ToString();
            }



            if (CoreProfile.ContainsKey(CoreProfileAttributeName.ExpressionProfile_ResourceId))
            {
                NSMessageHandler.ContactService.Deltas.Profile.HasExpressionProfile = true;
                if (NSMessageHandler.ContactService.Deltas.Profile.ExpressionProfile == null)
                    NSMessageHandler.ContactService.Deltas.Profile.ExpressionProfile = new ProfileResource();
                NSMessageHandler.ContactService.Deltas.Profile.ExpressionProfile.ResourceID = CoreProfile[CoreProfileAttributeName.ExpressionProfile_ResourceId].ToString();

                if (CoreProfile.ContainsKey(CoreProfileAttributeName.PictureProfile_UserTileStatic_ResourceId))
                    NSMessageHandler.ContactService.Deltas.Profile.Photo.ResourceID = CoreProfile[CoreProfileAttributeName.PictureProfile_UserTileStatic_ResourceId].ToString();



                if (CoreProfile.ContainsKey(CoreProfileAttributeName.ExpressionProfile_DisplayName_LastModified))
                    NSMessageHandler.ContactService.Deltas.Profile.ExpressionProfile.DateModified = CoreProfile[CoreProfileAttributeName.ExpressionProfile_DisplayName_LastModified].ToString();

                if (CoreProfile.ContainsKey(CoreProfileAttributeName.ExpressionProfile_PersonalStatus))
                {
                    NSMessageHandler.ContactService.Deltas.Profile.PersonalMessage = CoreProfile[CoreProfileAttributeName.ExpressionProfile_PersonalStatus].ToString();
                    PersonalMessage newPersonalMessage = PersonalMessage == null ? new PersonalMessage(NSMessageHandler.ContactService.Deltas.Profile.PersonalMessage) : PersonalMessage;
                    newPersonalMessage.Message = NSMessageHandler.ContactService.Deltas.Profile.PersonalMessage;
                    PersonalMessage = newPersonalMessage;
                }
            }

            NSMessageHandler.ContactService.Deltas.Profile.DisplayName = Name;
            NSMessageHandler.ContactService.Deltas.Save(true);

            if (CoreProfile.ContainsKey(CoreProfileAttributeName.UserTileStaticUrl))
            {
                NSMessageHandler.StorageService.SyncUserTile(CoreProfile[CoreProfileAttributeName.UserTileStaticUrl].ToString(), true,
                    delegate(object param)
                    {
                        SerializableMemoryStream ms = param as SerializableMemoryStream;
                        if (ms != null)
                        {
                            NSMessageHandler.ContactService.Deltas.Profile.Photo.DisplayImage = ms;
                            NSMessageHandler.ContactService.Deltas.Save(true);
                        }

                        Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo, "Get owner's display image from: " + CoreProfile[CoreProfileAttributeName.UserTileStaticUrl] +
                            " succeeded.");
                    },
                    delegate(object param)
                    {
                        Exception ex = param as Exception;
                        if (ex != null)
                        {
                            Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "An error occurred while getting owner's display image from:" +
                                CoreProfile[CoreProfileAttributeName.UserTileStaticUrl] + "\r\n" +
                                ex.Message);
                        }
                    }
                    );
            }

            if (Name != PreferredName)
            {
                try
                {
                    NSMessageHandler.SetScreenName(PreferredName);
                    SetName(PreferredName);
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(Settings.TraceSwitch.TraceError, ex.Message);
                }
            }
        }

        /// <summary>
        /// Called when the server has send a profile description.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnProfileReceived(EventArgs e)
        {
            if (ProfileReceived != null)
                ProfileReceived(this, e);
        }

        protected internal override void OnCoreProfileUpdated(EventArgs e)
        {
            if (NSMessageHandler.ContactService.Deltas != null)
                SyncProfileToDeltas();

            base.OnCoreProfileUpdated(e);
        }

    }
};
