﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 
namespace MSNPSharp.LiveConnectAPI.Atom {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.w3.org/2005/Atom")]
    [System.Xml.Serialization.XmlRootAttribute("entry", Namespace="http://www.w3.org/2005/Atom", IsNullable=false)]
    public partial class entryType {
        
        private string idField;
        
        private string updatedField;
        
        private string legacyPsmField;
        
        private entryTypeTitle titleField;
        
        private authorType[] authorField;
        
        private linkType[] linkField;
        
        private richPsm richPsmField;
        
        /// <remarks/>
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        public string updated {
            get {
                return this.updatedField;
            }
            set {
                this.updatedField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://api.live.com/schemas")]
        public string legacyPsm {
            get {
                return this.legacyPsmField;
            }
            set {
                this.legacyPsmField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public entryTypeTitle title {
            get {
                return this.titleField;
            }
            set {
                this.titleField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("author")]
        public authorType[] author {
            get {
                return this.authorField;
            }
            set {
                this.authorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("link")]
        public linkType[] link {
            get {
                return this.linkField;
            }
            set {
                this.linkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://api.live.com/schemas")]
        public richPsm richPsm {
            get {
                return this.richPsmField;
            }
            set {
                this.richPsmField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.w3.org/2005/Atom")]
    public partial class entryTypeTitle {
        
        private string typeField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.w3.org/2005/Atom")]
    public partial class linkType {
        
        private string hrefField;
        
        private string relField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string href {
            get {
                return this.hrefField;
            }
            set {
                this.hrefField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string rel {
            get {
                return this.relField;
            }
            set {
                this.relField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.w3.org/2005/Atom")]
    public partial class authorType {
        
        private string nameField;
        
        private string cidField;
        
        /// <remarks/>
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://api.live.com/schemas")]
        public string cid {
            get {
                return this.cidField;
            }
            set {
                this.cidField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://api.live.com/schemas")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://api.live.com/schemas", IsNullable=false)]
    public partial class richPsm {
        
        private string richUserMsgField;
        
        /// <remarks/>
        public string richUserMsg {
            get {
                return this.richUserMsgField;
            }
            set {
                this.richUserMsgField = value;
            }
        }
    }
}
