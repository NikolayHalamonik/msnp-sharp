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

namespace MSNPSharp
{
    using MSNPSharp.Core;


    /// <summary>
    /// Membership type. The values of fields in this class is just as the same as their names.
    /// </summary>
    public static class MembershipType
    {
        public const string Passport = "Passport";
        public const string Email = "Email";
        public const string Phone = "Phone";
        public const string Role = "Role";
        public const string Service = "Service";
        public const string Everyone = "Everyone";
        public const string Partner = "Partner";
        public const string Domain = "Domain";
        public const string Circle = "Circle";
        public const string Group = "Group";
        public const string Guid = "Guid";
        public const string ExternalID = "ExternalID";
    }

    public static class MessengerContactType
    {
        public const string Me = "Me";
        public const string Regular = "Regular";
        public const string Messenger = "Messenger";
        public const string Live = "Live";
        public const string LivePending = "LivePending";
        public const string LiveRejected = "LiveRejected";
        public const string LiveDropped = "LiveDropped";
        public const string Circle = "Circle";
    }

    [Flags]
    public enum ServiceShortNames
    {
        /// <summary>
        /// Peer to Peer (PE)
        /// </summary>
        PE = 1,
        /// <summary>
        /// Instant Messaging (IM)
        /// </summary>
        IM = 2,
        /// <summary>
        /// Private Data (for owner endpoint places)
        /// </summary>
        PD = 4,
        CM = 8,
        /// <summary>
        /// Profile (PF)
        /// </summary>
        PF = 16,
    }

    public static class ContactPhoneTypes
    {
        public const string ContactPhonePersonal = "ContactPhonePersonal";
        public const string ContactPhoneBusiness = "ContactPhoneBusiness";
        public const string ContactPhoneMobile = "ContactPhoneMobile";
        public const string ContactPhonePager = "ContactPhonePager";
        public const string ContactPhoneOther = "ContactPhoneOther";
        public const string ContactPhoneFax = "ContactPhoneFax";
        public const string Personal2 = "Personal2";
        public const string Business2 = "Business2";
        public const string BusinessFax = "BusinessFax";
        public const string BusinessMobile = "BusinessMobile";
        public const string Company = "Company";
    }

    /// <summary>
    /// Property string for <see cref="MSNPSharp.MSNWS.MSNABSharingService.ContactType"/>
    /// </summary>
    public static class PropertyString
    {
        public const string propertySeparator = " ";
        public const string Email = "Email";
        public const string IsMessengerEnabled = "IsMessengerEnabled";
        public const string Capability = "Capability";
        public const string Number = "Number";
        public const string Comment = "Comment";
        public const string DisplayName = "DisplayName";
        public const string Annotation = "Annotation";
        public const string IsMessengerUser = "IsMessengerUser";
        public const string MessengerMemberInfo = "MessengerMemberInfo";
        public const string ContactType = "ContactType";
        public const string ContactEmail = "ContactEmail";
        public const string ContactPhone = "ContactPhone";
        public const string GroupName = "GroupName";
        public const string HasSpace = "HasSpace";
    }

    public enum MsnServiceType
    {
        AB,
        Sharing,
        Storage,
        WhatsUp,
        Directory
    }

    public static class PartnerScenario
    {
        public const string None = "None";
        public const string Initial = "Initial";
        public const string Timer = "Timer";
        public const string BlockUnblock = "BlockUnblock";
        public const string GroupSave = "GroupSave";
        public const string GeneralDialogApply = "GeneralDialogApply";
        public const string ContactSave = "ContactSave";
        public const string ContactMsgrAPI = "ContactMsgrAPI";
        public const string MessengerPendingList = "MessengerPendingList";
        public const string PrivacyApply = "PrivacyApply";
        public const string NewCircleDuringPull = "NewCircleDuringPull";
        public const string CircleInvite = "CircleInvite";
        public const string CircleIdAlert = "CircleIdAlert";
        public const string CircleStatus = "CircleStatus";
        public const string CircleSave = "CircleSave";
        public const string CircleLeave = "CircleLeave";
        public const string JoinedCircleDuringPush = "JoinedCircleDuringPush";
        public const string ABChangeNotifyAlert = "ABChangeNotifyAlert";
        public const string RoamingSeed = "RoamingSeed";
        public const string RoamingIdentityChanged = "RoamingIdentityChanged";
        public const string LivePlatformSyncChangesToServer0 = @"LivePlatform!SyncChangesToServer(0)";
    }

    public static class CoreProfileAttributeName
    {
        public const string PublicProfile_ResourceId = "PublicProfile.ResourceId";
        public const string UserTileStaticUrl = "UserTileStaticUrl";
        public const string UserTileStaticHash = "UserTileStaticHash";
        public const string UserTileStaticSize = "UserTileStaticSize";
        public const string ProfilePage = "ProfilePage";
        public const string LastModified = "LastModified";
        public const string LCid = "LCid";

        public const string PictureProfile_UserTileStatic_ResourceId = "PictureProfile.UserTileStatic.ResourceId";
        

        public const string ExpressionProfile_ResourceId = "ExpressionProfile.ResourceId";
        public const string ExpressionProfile_DisplayName = "ExpressionProfile.DisplayName";
        public const string ExpressionProfile_DisplayName_LastModified = "ExpressionProfile.DisplayName.LastModified";
        public const string ExpressionProfile_PersonalStatus = "ExpressionProfile.PersonalStatus";
        public const string ExpressionProfile_PersonalStatus_LastModified = "ExpressionProfile.PersonalStatus.LastModified";
        
        public const string PublicProfile_DisplayName = "PublicProfile.DisplayName";
        public const string PublicProfile_DisplayName_LastModified = "PublicProfile.DisplayName.LastModified";
        public const string PublicProfile_DisplayLastName = "PublicProfile.DisplayLastName";
        public const string PublicProfile_DisplayLastName_LastModified = "PublicProfile.DisplayLastName.LastModified";
    }

    /// <summary>
    /// Constants for webservice parameter.
    /// </summary>
    public static class WebServiceConstants
    {
        /// <summary>
        /// The messenger's default addressbook Id: 00000000-0000-0000-0000-000000000000.
        /// </summary>
        public const string MessengerIndividualAddressBookId = "00000000-0000-0000-0000-000000000000";

        /// <summary>
        /// The guid for messenger group(not circle): C8529CE2-6EAD-434d-881F-341E17DB3FF8.
        /// </summary>
        public const string MessengerGroupType = "C8529CE2-6EAD-434d-881F-341E17DB3FF8";

        /// <summary>
        /// The default time for requesting the full membership and addressbook list: 0001-01-01T00:00:00.0000000.
        /// </summary>
        public const string ZeroTime = "0001-01-01T00:00:00.0000000";

        public static string[] XmlDateTimeFormats = new string[]{
            "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-MM-ddTHH:mm:sszzz"
        };

        public const string NullDomainTag = "$null";


    }

    /// <summary>
    /// Different string for Name property of <see cref="MSNPSharp.MSNWS.MSNABSharingService.Annotation"/>
    /// </summary>
    public static class AnnotationNames
    {
        /// <summary>
        /// The value is: MSN.IM.InviteMessage
        /// </summary>
        public const string MSN_IM_InviteMessage = "MSN.IM.InviteMessage";

        /// <summary>
        /// The value is: MSN.IM.MPOP
        /// </summary>
        [Obsolete("",true)]
        public const string MSN_IM_MPOP = "MSN.IM.MPOP";

        /// <summary>
        /// The value is: MSN.IM.BLP
        /// </summary>
        [Obsolete("",true)]
        public const string MSN_IM_BLP = "MSN.IM.BLP";

        /// <summary>
        /// The value is: MSN.IM.GTC
        /// </summary>
        [Obsolete("", true)]
        public const string MSN_IM_GTC = "MSN.IM.GTC";

        /// <summary>
        /// The value is: MSN.IM.RoamLiveProperties
        /// </summary>
        [Obsolete("", true)]
        public const string MSN_IM_RoamLiveProperties = "MSN.IM.RoamLiveProperties";

        /// <summary>
        /// The value is: MSN.IM.MBEA
        /// </summary>
        [Obsolete("", true)]
        public const string MSN_IM_MBEA = "MSN.IM.MBEA";

        /// <summary>
        /// The value is: MSN.IM.Display
        /// </summary>
        public const string MSN_IM_Display = "MSN.IM.Display";

        /// <summary>
        /// The value is: MSN.IM.BuddyType
        /// </summary>
        public const string MSN_IM_BuddyType = "MSN.IM.BuddyType";

        /// <summary>
        /// The value is: AB.NickName
        /// </summary>
        public const string AB_NickName = "AB.NickName";

        /// <summary>
        /// The value is: AB.Profession
        /// </summary>
        public const string AB_Profession = "AB.Profession";

        /// <summary>
        /// The value is: Live.Locale
        /// </summary>
        public const string Live_Locale = "Live.Locale";

        /// <summary>
        /// The value is: Live.Profile.Expression.LastChanged
        /// </summary>
        public const string Live_Profile_Expression_LastChanged = "Live.Profile.Expression.LastChanged";

        /// <summary>
        /// The value is: Live.Passport.Birthdate
        /// </summary>
        public const string Live_Passport_Birthdate = "Live.Passport.Birthdate";
    }


    /// <summary>
    /// The type of addressbook.
    /// </summary>
    public static class AddressBookType
    {
        /// <summary>
        /// Circle.
        /// </summary>
        public const string Group = "Group";

        /// <summary>
        /// Default addressbook.
        /// </summary>
        public const string Individual = "Individual";
    }

    /// <summary>
    /// This is the value of different domain type of Network info list.
    /// </summary>
    internal static class DomainIds
    {
        /// <summary>
        /// Domain id for Windows Live addressbook in NetworkInfo.
        /// </summary>
        public const int WindowsLiveDomain = 1;

        /// <summary>
        /// Domain ID for facebook in NetworkInfo.
        /// </summary>
        public const int FaceBookDomain = 7;
        public const int ZUNEDomain = 3;

        public const int LinkedInDomain = 8;
        /// <summary>
        /// The domain ID for MySpace.
        /// </summary>
        public const int MySpaceDomain = 9;
    }

    /// <summary>
    /// The values in this class might be different from <see cref="RemoteNetworkGateways"/>
    /// </summary>
    public static class SourceId
    {
        public const string WindowsLive = "WL";
        /// <summary>
        /// The source Id for facebook, "FB".
        /// </summary>
        public const string FaceBook = "FB";
        /// <summary>
        /// The source Id for MySpace, "MYSP".
        /// </summary>
        public const string MySpace = "MYSP";
        /// <summary>
        /// The source Id for LinkedIn, "LI".
        /// </summary>
        public const string LinkedIn = "LI";
    }

    public static class URLType
    {
        public const string Other = "Other";
    }

    public static class URLName
    {
        public const string UserTileL = "UserTile:L";
        public const string UserTileXL = "UserTile:XL";
    }

    /// <summary>
    /// The addressbook relationship types.
    /// </summary>
    internal static class RelationshipTypes
    {
        /// <summary>
        /// The network info relationship is for individual addressbook (default addressbook).
        /// </summary>
        public const int IndividualAddressBook = 3;

        /// <summary>
        /// The network info relationship is for group addressbook (circle addressbook).
        /// </summary>
        public const int CircleGroup = 5;
    }

    /// <summary>
    /// Indicates the status of  contact in an addressbook.
    /// </summary>
    internal enum RelationshipState : uint
    {
        None = 0,

        /// <summary>
        /// The remote circle owner invite you to join,, pending your response.
        /// </summary>
        WaitingResponse = 1,

        /// <summary>
        /// The contact is deleted by one of the domain owners.
        /// </summary>
        Left = 2,

        /// <summary>
        /// The contact is in the circle's addressbook list.
        /// </summary>
        Accepted = 3,

        /// <summary>
        /// The contact already left the circle.
        /// </summary>
        Rejected = 4
    }
};
