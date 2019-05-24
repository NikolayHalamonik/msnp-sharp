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
using System.Web;
using System.Xml;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace MSNPSharp
{
    using MSNPSharp.Core;

    /// <summary>
    /// Defines the type of MSNObject.
    /// <para>Thanks for ZoroNaX : http://zoronax.spaces.live.com/blog/cns!4A0B813054895814!180.entry </para>
    /// </summary>
    public enum MSNObjectType
    {
        /// <summary>
        /// Unknown msnobject type.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Avatar, Unknown
        /// </summary>
        Avatar = 1,
        /// <summary>
        /// Emotion icon.
        /// </summary>
        Emoticon = 2,
        /// <summary>
        /// User display image.
        /// </summary>
        UserDisplay = 3,
        /// <summary>
        /// ShareFile, Unknown
        /// </summary>
        ShareFile = 4,
        /// <summary>
        /// Background image.
        /// </summary>
        Background = 5,
        /// <summary>
        /// History
        /// </summary>
        History = 6,
        /// <summary>
        /// Deluxe Display Pictures
        /// </summary>
        DynamicPicture = 7,
        /// <summary>
        /// flash emoticon
        /// </summary>
        Wink = 8,
        /// <summary>
        /// Map File  A map file contains a list of items in the store.
        /// </summary>
        MapFile = 9,
        /// <summary>
        /// Dynamic Backgrounds
        /// </summary>
        DynamicBackground = 10,
        /// <summary>
        /// Voice Clip
        /// </summary>
        VoiceClip = 11,
        /// <summary>
        /// Plug-In State. Saved state of Add-ins.
        /// </summary>
        SavedState = 12,
        /// <summary>
        /// Roaming Objects. For example, your roaming display picture.
        /// </summary>
        RoamingObject = 13,
        /// <summary>
        /// Signature sound
        /// </summary>
        SignatureSound = 14,
        /// <summary>
        /// Scene image
        /// </summary>
        Scene = 16
    }

    /// <summary>
    /// The MSNObject can hold an image, display, emoticon, etc.
    /// </summary>
    [Serializable()]
    public class MSNObject
    {
        private string oldHash = string.Empty;

        [NonSerialized]
        private PersistentStream dataStream = null;


        private string fileLocation = string.Empty;
        private string creator = string.Empty;
        private int size = 0;
        private MSNObjectType type = MSNObjectType.Unknown;
        private string location = string.Empty;
        private string sha = string.Empty;

        private object syncObject = new object();

        public object SyncObject
        {
            get
            {
                return syncObject;
            }
        }

        /// <summary>
        /// The datastream to write to, or to read from
        /// </summary>
        protected PersistentStream DataStream
        {
            get
            {
                lock (SyncObject)
                    return dataStream;
            }
            set
            {
                lock (SyncObject)
                {
                    if (dataStream != null)
                        dataStream.Close();

                    dataStream = value;
                    UpdateStream();
                }
            }
        }

        /// <summary>
        /// Update the size and SHA info after the DataStream property has been changed.
        /// </summary>
        protected void UpdateStream()
        {
            if (DataStream != null)
            {
                Size = (int)DataStream.Length;
                Sha = GetStreamHash(DataStream);
            }
        }

        /// <summary>
        /// The local contact list owner
        /// </summary>
        public string Creator
        {
            get
            {
                return creator;
            }
            set
            {
                creator = value;
                UpdateInCollection();
            }
        }

        /// <summary>
        /// The total data size
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }

            private set
            {
                size = value;
                UpdateInCollection();
            }
        }

        /// <summary>
        /// The type of MSN Object
        /// </summary>
        public MSNObjectType ObjectType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                UpdateInCollection();
            }
        }

        /// <summary>
        /// The location of the object. This is a location on the hard-drive. Use relative paths. This is only a text string; na data is read in after setting this field. Use FileLocation for that purpose.
        /// </summary>
        public string Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
                UpdateInCollection();
            }
        }

        /// <summary>
        /// [Deprecated, use LoadFile()] Gets or sets the file location. When a file is set the file data is immediately read in memory to extract the filehash. It will retain in memory afterwards.
        /// </summary>        
        public string FileLocation
        {
            get
            {
                return fileLocation;
            }
            set
            {
                this.LoadFile(value);
                fileLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the file location. When a file is set the file data is immediately read in memory to extract the filehash. It will retain in memory afterwards.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFile(string fileName)
        {
            FileInfo finfo = new FileInfo(fileName);
            if (finfo.Exists)
            {
                if (this.fileLocation == fileName)
                    return;
                this.fileLocation = fileName;
                this.location = Path.GetRandomFileName();

                // and open a new stream
                PersistentStream persistentStream = new PersistentStream(new MemoryStream());

                // copy the file
                byte[] buffer = new byte[512];
                Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                int cnt = 0;
                while ((cnt = fileStream.Read(buffer, 0, 512)) > 0)
                {
                    persistentStream.Write(buffer, 0, cnt);
                }

                DataStream = persistentStream;

                UpdateInCollection();
            }
        }

        /// <summary>
        /// The SHA1 encrypted hash of the datastream.
        /// </summary>
        /// <remarks>
        /// Usually the application programmer don't need to set this itself.
        /// </remarks>
        public string Sha
        {
            get
            {
                return sha;
            }

            set
            {
                sha = value;
                UpdateInCollection();
            }
        }

        /// <summary>
        /// Updates the msn object in the global MSNObjectCatalog.
        /// </summary>
        public void UpdateInCollection()
        {
            if (oldHash.Length != 0)
                MSNObjectCatalog.GetInstance().Remove(oldHash);

            oldHash = CalculateChecksum();

            MSNObjectCatalog.GetInstance().Add(oldHash, this);
        }

        /// <summary>
        /// Calculates the hash of datastream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static string GetStreamHash(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            // fet file hash
            byte[] bytes = new byte[(int)stream.Length];

            //put bytes into byte array
            stream.Read(bytes, 0, (int)stream.Length);

            //create SHA1 object 
            HashAlgorithm hash = new SHA1Managed();
            byte[] hashBytes = hash.ComputeHash(bytes);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Creates a MSNObject.
        /// </summary>		
        public MSNObject()
        {
            DataStream = new PersistentStream(new MemoryStream());
        }

        /// <summary>
        /// Parses a context send by the remote contact and set the corresponding class variables. Context input is assumed to be not base64 encoded.
        /// </summary>
        /// <param name="context"></param>
        public virtual void SetContext(string context)
        {
            SetContext(context, false);
        }

        /// <summary>
        /// Parses a context send by the remote contact and set the corresponding class variables.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base64Encoded"></param>
        public virtual void SetContext(string context, bool base64Encoded)
        {
            if (base64Encoded)
                context = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(context));

            string xmlString = GetDecodeString(context);

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);
                XmlNode msnObjNode = xmlDoc.SelectSingleNode("//msnobj");

                foreach (XmlNode attr in msnObjNode.Attributes)
                {
                    string val = attr.Value;
                    switch (attr.Name.ToLowerInvariant())
                    {
                        case "creator":
                            this.creator = val;
                            break;
                        case "size":
                            this.size = int.Parse(val, System.Globalization.CultureInfo.InvariantCulture);
                            break;
                        case "type":
                            {
                                switch (val)
                                {
                                    case "2":
                                        type = MSNObjectType.Emoticon;
                                        break;
                                    case "3":
                                        type = MSNObjectType.UserDisplay;
                                        break;
                                    case "5":
                                        type = MSNObjectType.Background;
                                        break;
                                    case "8":
                                        type = MSNObjectType.Wink;
                                        break;
                                    case "16":
                                        type = MSNObjectType.Scene;
                                        break;
                                }
                                break;
                            }
                        case "location":
                            this.location = val;
                            break;
                        case "sha1d":
                            this.sha = val.Replace(' ', '+');
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "MSNObject Set Conext error: context " +
                    xmlString + " is not a valid context for MSNObject.\r\n  Error description: " +
                    ex.Message + "\r\n  Stack Trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Constructs a MSN object based on a (memory)stream. The client programmer is responsible for inserting this object in the global msn object collection.
        /// The stream must remain open during the whole life-length of the application.
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="inputStream"></param>
        /// <param name="type"></param>
        /// <param name="location"></param>
        public MSNObject(string creator, Stream inputStream, MSNObjectType type, string location)
        {
            this.creator = creator;
            this.size = (int)inputStream.Length;
            this.type = type;
            this.location = location;

            this.sha = GetStreamHash(inputStream);

            this.DataStream = new PersistentStream(inputStream);
        }

        /// <summary>
        /// Constructs a MSN object based on a physical file. The client programmer is responsible for inserting this object in the global msn object collection.
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="type"></param>
        /// <param name="fileLocation"></param>
        public MSNObject(string creator, string fileLocation, MSNObjectType type)
        {
            this.location = Path.GetFullPath(fileLocation).Replace(Path.GetPathRoot(fileLocation), "");
            this.location += new Random().Next().ToString(CultureInfo.InvariantCulture);

            this.fileLocation = fileLocation;

            Stream stream = OpenStream();

            this.creator = creator;
            this.size = (int)stream.Length;
            this.type = type;

            this.sha = GetStreamHash(stream);
            stream.Close();
        }

        /// <summary>
        /// Returns the stream to read from. In case of an in-memory stream that stream is returned. In case of a filelocation
        /// a stream to the file will be opened and returned. The stream is not guaranteed to positioned at the beginning of the stream.
        /// </summary>
        /// <returns></returns>
        public virtual Stream OpenStream()
        {
            DataStream.Open();

            // otherwise it's a memorystream
            return DataStream;
        }

        /// <summary>
        /// Returns the "url-encoded xml" string for MSNObjects.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetEncodeString(string context)
        {
            return MSNHttpUtility.MSNObjectUrlEncode(context);
        }

        public static string GetDecodeString(string context)
        {
            if (context.IndexOf(" ") == -1)
            {
                return MSNHttpUtility.MSNObjectUrlDecode(context);
            }
            return context;
        }


        /// <summary>
        /// Calculates the checksum for the entire MSN Object.
        /// </summary>
        /// <remarks>This value is used to uniquely identify a MSNObject.</remarks>
        /// <returns></returns>
        public string CalculateChecksum()
        {
            string checksum = "Creator" + Creator + "Size" + Size + "Type" + (int)this.ObjectType + "Location" + HttpUtility.UrlEncode(Location) + "FriendlyAAA=SHA1D" + Sha;

            HashAlgorithm shaAlg = new SHA1Managed();
            string baseEncChecksum = Convert.ToBase64String(shaAlg.ComputeHash(Encoding.UTF8.GetBytes(checksum)));
            return baseEncChecksum;
        }

        /// <summary>
        /// The context as an url-encoded xml string.
        /// </summary>
        public string Context
        {
            get
            {
                return GetEncodedString();
            }
        }

        /// <summary>
        /// The context as an xml string, not url-encoded.
        /// </summary>
        public string ContextPlain
        {
            get
            {
                return GetXmlString();
            }
        }

        /// <summary>
        /// Returns the xml string.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetXmlString()
        {
            return "<msnobj Creator=\"" + Creator + "\" Size=\"" + Size + "\" Type=\"" + (int)this.ObjectType + "\" Location=\"" + HttpUtility.UrlEncode(Location) + "\" Friendly=\"AAA=\" SHA1D=\"" + Sha + "\" SHA1C=\"" + CalculateChecksum() + "\"" + " />";
        }

        /// <summary>
        /// Returns the url-encoded xml string.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetEncodedString()
        {
            return MSNHttpUtility.MSNObjectUrlEncode(GetXmlString());
        }


        protected virtual bool ContextEqual(string contextPlain)
        {
            if (Size == 0 && (string.IsNullOrEmpty(contextPlain) || contextPlain == "0"))
                return true;

            try
            {
                XmlDocument msnObjectDocument = new XmlDocument();
                msnObjectDocument.LoadXml(contextPlain);
                XmlNode msnObjectNode = msnObjectDocument.SelectSingleNode("msnobj");
                string sha = msnObjectNode.Attributes["SHA1D"].InnerText;
                string creator = msnObjectNode.Attributes["Creator"].InnerText;
                MSNObjectType type = (MSNObjectType)int.Parse(msnObjectNode.Attributes["Type"].InnerText);

                return (Sha == sha && Creator == creator && type == ObjectType);

            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "MSNObject compare error: context " +
                    contextPlain + " is not a valid context for MSNObject.\r\n  Error description: " +
                    ex.Message + "\r\n  Stack Trace: " + ex.StackTrace);
                return true;
            }
        }


        protected virtual bool MSNObjectEqual(MSNObject obj2)
        {
            if ((object)obj2 == null)
                return false;

            return GetHashCode() == obj2.GetHashCode();
        }


        public static bool operator ==(MSNObject msnObject, object compareTarget)
        {
            if ((object)msnObject == null && compareTarget == null)
                return true;

            if ((object)msnObject == null || compareTarget == null)
                return false;

            return msnObject.Equals(compareTarget);
        }

        public static bool operator !=(MSNObject msnObject, object compareTarget)
        {
            return !(msnObject == compareTarget);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj == null || obj is MSNObject)
                return MSNObjectEqual(obj as MSNObject);

            if (obj is string)
                return ContextEqual(GetDecodeString(obj.ToString()));

            return false;
        }

        public override int GetHashCode()
        {
            return CalculateChecksum().GetHashCode();
        }
    }
};
