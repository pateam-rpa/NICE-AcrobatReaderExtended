using Direct.Shared;


namespace Direct.PDFExtended.Library
{
    #region Supporting Classes

    [DirectDom("PDF Field", "General")]
    public class PDFField : DirectComponentBase
    {
        protected PropertyHolder<string> _Name = new PropertyHolder<string>("Name");
        protected CompositePropertyHolder<FieldSize> _Size = new CompositePropertyHolder<FieldSize>("Size");
        protected CompositePropertyHolder<FieldPosition> _Position = new CompositePropertyHolder<FieldPosition>("Position");
        protected CompositePropertyHolder<FieldProperties> _FieldProperties = new CompositePropertyHolder<FieldProperties>("Properties");


        [DirectDom("Name")]
        [DesignTimeInfo("Name")]
        public string Name
        {
            get { return _Name.TypedValue; }
            set { _Name.TypedValue = value; }
        }

        [DirectDom("Size")]
        [DesignTimeInfo("Size")]
        public FieldSize Size
        {
            get
            {
                if (_Size.TypedValue == null)
                {
                    _Size.TypedValue = new FieldSize(Project);
                }
                return _Size.TypedValue;
            }
            set { _Size.TypedValue = value; }
        }

        [DirectDom("Position")]
        [DesignTimeInfo("Position")]
        public FieldPosition Position
        {
            get
            {
                if (_Position.TypedValue == null)
                {
                    _Position.TypedValue = new FieldPosition(Project);
                }
                return _Position.TypedValue;
            }
            set { _Position.TypedValue = value; }
        }

        [DirectDom("Properties")]
        [DesignTimeInfo("Properties")]
        public FieldProperties FieldProperties
        {
            get
            {
                if (_FieldProperties.TypedValue == null)
                {
                    _FieldProperties.TypedValue = new FieldProperties(Project);
                }
                return _FieldProperties.TypedValue;
            }
            set { _FieldProperties.TypedValue = value; }
        }

        public PDFField()
        {

        }

        public PDFField(IProject project) : base(project)
        {

        }

    }

    #endregion
}