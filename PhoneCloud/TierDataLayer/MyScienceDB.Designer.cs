﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;

[assembly: EdmSchemaAttribute()]

namespace TierDataLayer
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class MyScienceEntities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new MyScienceEntities object using the connection string found in the 'MyScienceEntities' section of the application configuration file.
        /// </summary>
        public MyScienceEntities() : base("name=MyScienceEntities", "MyScienceEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new MyScienceEntities object.
        /// </summary>
        public MyScienceEntities(string connectionString) : base(connectionString, "MyScienceEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new MyScienceEntities object.
        /// </summary>
        public MyScienceEntities(EntityConnection connection) : base(connection, "MyScienceEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<coordinator> coordinators
        {
            get
            {
                if ((_coordinators == null))
                {
                    _coordinators = base.CreateObjectSet<coordinator>("coordinators");
                }
                return _coordinators;
            }
        }
        private ObjectSet<coordinator> _coordinators;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<datum> data
        {
            get
            {
                if ((_data == null))
                {
                    _data = base.CreateObjectSet<datum>("data");
                }
                return _data;
            }
        }
        private ObjectSet<datum> _data;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<projectfield> projectfields
        {
            get
            {
                if ((_projectfields == null))
                {
                    _projectfields = base.CreateObjectSet<projectfield>("projectfields");
                }
                return _projectfields;
            }
        }
        private ObjectSet<projectfield> _projectfields;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<project> projects
        {
            get
            {
                if ((_projects == null))
                {
                    _projects = base.CreateObjectSet<project>("projects");
                }
                return _projects;
            }
        }
        private ObjectSet<project> _projects;
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<user> users
        {
            get
            {
                if ((_users == null))
                {
                    _users = base.CreateObjectSet<user>("users");
                }
                return _users;
            }
        }
        private ObjectSet<user> _users;

        #endregion
        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the coordinators EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddTocoordinators(coordinator coordinator)
        {
            base.AddObject("coordinators", coordinator);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the data EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddTodata(datum datum)
        {
            base.AddObject("data", datum);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the projectfields EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToprojectfields(projectfield projectfield)
        {
            base.AddObject("projectfields", projectfield);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the projects EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToprojects(project project)
        {
            base.AddObject("projects", project);
        }
    
        /// <summary>
        /// Deprecated Method for adding a new object to the users EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddTousers(user user)
        {
            base.AddObject("users", user);
        }

        #endregion
    }
    

    #endregion
    
    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="MyScienceModel", Name="coordinator")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class coordinator : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new coordinator object.
        /// </summary>
        /// <param name="id">Initial value of the ID property.</param>
        public static coordinator Createcoordinator(global::System.Int32 id)
        {
            coordinator coordinator = new coordinator();
            coordinator.ID = id;
            return coordinator;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (_ID != value)
                {
                    OnIDChanging(value);
                    ReportPropertyChanging("ID");
                    _ID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ID");
                    OnIDChanged();
                }
            }
        }
        private global::System.Int32 _ID;
        partial void OnIDChanging(global::System.Int32 value);
        partial void OnIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String name
        {
            get
            {
                return _name;
            }
            set
            {
                OnnameChanging(value);
                ReportPropertyChanging("name");
                _name = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("name");
                OnnameChanged();
            }
        }
        private global::System.String _name;
        partial void OnnameChanging(global::System.String value);
        partial void OnnameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String email
        {
            get
            {
                return _email;
            }
            set
            {
                OnemailChanging(value);
                ReportPropertyChanging("email");
                _email = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("email");
                OnemailChanged();
            }
        }
        private global::System.String _email;
        partial void OnemailChanging(global::System.String value);
        partial void OnemailChanged();

        #endregion
    
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="MyScienceModel", Name="datum")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class datum : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new datum object.
        /// </summary>
        /// <param name="id">Initial value of the ID property.</param>
        /// <param name="projectid">Initial value of the projectid property.</param>
        /// <param name="userid">Initial value of the userid property.</param>
        /// <param name="data">Initial value of the data property.</param>
        /// <param name="time">Initial value of the time property.</param>
        /// <param name="location">Initial value of the location property.</param>
        public static datum Createdatum(global::System.Int32 id, global::System.Int32 projectid, global::System.Int32 userid, global::System.String data, global::System.DateTime time, global::System.String location)
        {
            datum datum = new datum();
            datum.ID = id;
            datum.projectid = projectid;
            datum.userid = userid;
            datum.data = data;
            datum.time = time;
            datum.location = location;
            return datum;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (_ID != value)
                {
                    OnIDChanging(value);
                    ReportPropertyChanging("ID");
                    _ID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ID");
                    OnIDChanged();
                }
            }
        }
        private global::System.Int32 _ID;
        partial void OnIDChanging(global::System.Int32 value);
        partial void OnIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 projectid
        {
            get
            {
                return _projectid;
            }
            set
            {
                OnprojectidChanging(value);
                ReportPropertyChanging("projectid");
                _projectid = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("projectid");
                OnprojectidChanged();
            }
        }
        private global::System.Int32 _projectid;
        partial void OnprojectidChanging(global::System.Int32 value);
        partial void OnprojectidChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 userid
        {
            get
            {
                return _userid;
            }
            set
            {
                OnuseridChanging(value);
                ReportPropertyChanging("userid");
                _userid = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("userid");
                OnuseridChanged();
            }
        }
        private global::System.Int32 _userid;
        partial void OnuseridChanging(global::System.Int32 value);
        partial void OnuseridChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String data
        {
            get
            {
                return _data;
            }
            set
            {
                OndataChanging(value);
                ReportPropertyChanging("data");
                _data = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("data");
                OndataChanged();
            }
        }
        private global::System.String _data;
        partial void OndataChanging(global::System.String value);
        partial void OndataChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime time
        {
            get
            {
                return _time;
            }
            set
            {
                OntimeChanging(value);
                ReportPropertyChanging("time");
                _time = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("time");
                OntimeChanged();
            }
        }
        private global::System.DateTime _time;
        partial void OntimeChanging(global::System.DateTime value);
        partial void OntimeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String location
        {
            get
            {
                return _location;
            }
            set
            {
                OnlocationChanging(value);
                ReportPropertyChanging("location");
                _location = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("location");
                OnlocationChanged();
            }
        }
        private global::System.String _location;
        partial void OnlocationChanging(global::System.String value);
        partial void OnlocationChanged();

        #endregion
    
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="MyScienceModel", Name="project")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class project : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new project object.
        /// </summary>
        /// <param name="id">Initial value of the ID property.</param>
        /// <param name="name">Initial value of the name property.</param>
        /// <param name="description">Initial value of the description property.</param>
        /// <param name="owner">Initial value of the owner property.</param>
        /// <param name="form">Initial value of the form property.</param>
        public static project Createproject(global::System.Int32 id, global::System.String name, global::System.String description, global::System.Int32 owner, global::System.String form)
        {
            project project = new project();
            project.ID = id;
            project.name = name;
            project.description = description;
            project.owner = owner;
            project.form = form;
            return project;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (_ID != value)
                {
                    OnIDChanging(value);
                    ReportPropertyChanging("ID");
                    _ID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ID");
                    OnIDChanged();
                }
            }
        }
        private global::System.Int32 _ID;
        partial void OnIDChanging(global::System.Int32 value);
        partial void OnIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String name
        {
            get
            {
                return _name;
            }
            set
            {
                OnnameChanging(value);
                ReportPropertyChanging("name");
                _name = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("name");
                OnnameChanged();
            }
        }
        private global::System.String _name;
        partial void OnnameChanging(global::System.String value);
        partial void OnnameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String description
        {
            get
            {
                return _description;
            }
            set
            {
                OndescriptionChanging(value);
                ReportPropertyChanging("description");
                _description = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("description");
                OndescriptionChanged();
            }
        }
        private global::System.String _description;
        partial void OndescriptionChanging(global::System.String value);
        partial void OndescriptionChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 owner
        {
            get
            {
                return _owner;
            }
            set
            {
                OnownerChanging(value);
                ReportPropertyChanging("owner");
                _owner = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("owner");
                OnownerChanged();
            }
        }
        private global::System.Int32 _owner;
        partial void OnownerChanging(global::System.Int32 value);
        partial void OnownerChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String form
        {
            get
            {
                return _form;
            }
            set
            {
                OnformChanging(value);
                ReportPropertyChanging("form");
                _form = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("form");
                OnformChanged();
            }
        }
        private global::System.String _form;
        partial void OnformChanging(global::System.String value);
        partial void OnformChanged();

        #endregion
    
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="MyScienceModel", Name="projectfield")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class projectfield : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new projectfield object.
        /// </summary>
        /// <param name="id">Initial value of the ID property.</param>
        public static projectfield Createprojectfield(global::System.Int32 id)
        {
            projectfield projectfield = new projectfield();
            projectfield.ID = id;
            return projectfield;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (_ID != value)
                {
                    OnIDChanging(value);
                    ReportPropertyChanging("ID");
                    _ID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ID");
                    OnIDChanged();
                }
            }
        }
        private global::System.Int32 _ID;
        partial void OnIDChanging(global::System.Int32 value);
        partial void OnIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String name
        {
            get
            {
                return _name;
            }
            set
            {
                OnnameChanging(value);
                ReportPropertyChanging("name");
                _name = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("name");
                OnnameChanged();
            }
        }
        private global::System.String _name;
        partial void OnnameChanging(global::System.String value);
        partial void OnnameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> type
        {
            get
            {
                return _type;
            }
            set
            {
                OntypeChanging(value);
                ReportPropertyChanging("type");
                _type = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("type");
                OntypeChanged();
            }
        }
        private Nullable<global::System.Int32> _type;
        partial void OntypeChanging(Nullable<global::System.Int32> value);
        partial void OntypeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> projectid
        {
            get
            {
                return _projectid;
            }
            set
            {
                OnprojectidChanging(value);
                ReportPropertyChanging("projectid");
                _projectid = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("projectid");
                OnprojectidChanged();
            }
        }
        private Nullable<global::System.Int32> _projectid;
        partial void OnprojectidChanging(Nullable<global::System.Int32> value);
        partial void OnprojectidChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> order
        {
            get
            {
                return _order;
            }
            set
            {
                OnorderChanging(value);
                ReportPropertyChanging("order");
                _order = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("order");
                OnorderChanged();
            }
        }
        private Nullable<global::System.Int32> _order;
        partial void OnorderChanging(Nullable<global::System.Int32> value);
        partial void OnorderChanged();

        #endregion
    
    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="MyScienceModel", Name="user")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class user : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new user object.
        /// </summary>
        /// <param name="id">Initial value of the ID property.</param>
        /// <param name="phoneid">Initial value of the phoneid property.</param>
        /// <param name="name">Initial value of the name property.</param>
        public static user Createuser(global::System.Int32 id, global::System.String phoneid, global::System.String name)
        {
            user user = new user();
            user.ID = id;
            user.phoneid = phoneid;
            user.name = name;
            return user;
        }

        #endregion
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (_ID != value)
                {
                    OnIDChanging(value);
                    ReportPropertyChanging("ID");
                    _ID = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("ID");
                    OnIDChanged();
                }
            }
        }
        private global::System.Int32 _ID;
        partial void OnIDChanging(global::System.Int32 value);
        partial void OnIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String phoneid
        {
            get
            {
                return _phoneid;
            }
            set
            {
                OnphoneidChanging(value);
                ReportPropertyChanging("phoneid");
                _phoneid = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("phoneid");
                OnphoneidChanged();
            }
        }
        private global::System.String _phoneid;
        partial void OnphoneidChanging(global::System.String value);
        partial void OnphoneidChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String name
        {
            get
            {
                return _name;
            }
            set
            {
                OnnameChanging(value);
                ReportPropertyChanging("name");
                _name = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("name");
                OnnameChanged();
            }
        }
        private global::System.String _name;
        partial void OnnameChanging(global::System.String value);
        partial void OnnameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Int32> score
        {
            get
            {
                return _score;
            }
            set
            {
                OnscoreChanging(value);
                ReportPropertyChanging("score");
                _score = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("score");
                OnscoreChanged();
            }
        }
        private Nullable<global::System.Int32> _score;
        partial void OnscoreChanging(Nullable<global::System.Int32> value);
        partial void OnscoreChanged();

        #endregion
    
    }

    #endregion
    
}
