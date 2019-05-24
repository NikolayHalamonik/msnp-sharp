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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MSNPSharp
{
    using MSNPSharp.Core;

    [Serializable()]
    public class SceneImage : MSNObject
    {
        private Image image = null;
        private bool isDefaultImage = false;

        private static string defaultLocation = "SceneDefault";
        private static Image defaultImage = Properties.Resources.default_scene;

        public static Image DefaultImage
        {
            get
            {
                return defaultImage;
            }
        }
        

        public bool IsDefaultImage
        {
            get
            {
                return isDefaultImage;
            }
            protected internal set
            {
                isDefaultImage = value;
            }
        }

        public SceneImage()
            : this(string.Empty, false)
        {
        }

        public SceneImage(string creator)
            : this(creator, false)
        {
        }

        internal SceneImage(string creator, bool isDefault)
        {
            ObjectType = MSNObjectType.Scene;

            if (isDefault)
            {
                Location = defaultLocation;
                //PersistentStream stream = new PersistentStream(new MemoryStream());
                //(defaultImage.Clone() as Image).Save(stream, defaultImage.RawFormat);
                //DataStream = stream;
            }

            isDefaultImage = isDefault;
            Creator = creator;

            RetrieveImage();
        }

        public SceneImage(string creator, MemoryStream input, string location)
            : base(creator, new MemoryStream(input.ToArray()), MSNObjectType.Scene, location)
        {
            RetrieveImage();
        }

        public SceneImage(string creator, MemoryStream input)
            : this(creator, input, defaultLocation)
        {
        }

        public static SceneImage CreateDefaultImage(string creator)
        {
            return new SceneImage(creator, true);
        }

        public Image Image
        {
            get
            {
                lock (SyncObject)
                {
                    if (DataStream != null)
                        RetrieveImage();

                    return image == null ? null : image.Clone() as Image;
                }
            }
            internal protected set
            {
                image = value;
                image.Save(DataStream, image.RawFormat);

                RetrieveImage();
            }
        }

        public void RetrieveImage()
        {
            UpdateStream();

            Stream input = DataStream;

            if (input != null)
            {
                lock (input)
                {
                    input.Position = 0;
                    if (input.Length > 0)
                    {
                        lock (SyncObject)
                            image = System.Drawing.Image.FromStream(input);
                    }

                    input.Position = 0;
                }
            }
        }

        /// <summary>
        /// Get the raw stream data for saving.
        /// </summary>
        /// <returns></returns>
        internal byte[] GetRawData()
        {
            if (isDefaultImage)
                return null;

            if (DataStream == null)
                return null;

            if (OpenStream().CanRead)
            {
                byte[] data = new byte[Size];
                DataStream.Seek(0, SeekOrigin.Begin);
                DataStream.Read(data, 0, Size);
                DataStream.Close();
                return data;
            }

            DataStream.Close();
            return null;
        }

        protected override bool ContextEqual(string contextPlain)
        {
            if (isDefaultImage && string.IsNullOrEmpty(contextPlain))
                return true;

            return base.ContextEqual(contextPlain);
        }
    }
};
