using System.ComponentModel;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class BindableToolStripMenuItem : ToolStripMenuItem, IBindableComponent
    {
        private BindingContext _bindingContext;
        private ControlBindingsCollection _dataBindings;

        [Browsable(false)]
        public new BindingContext BindingContext
        {
            get
            {
                _bindingContext ??= new BindingContext();
                return _bindingContext;
            }
            set
            {
                _bindingContext = value;
            }
        }

        public bool ShouldSerializeBindingContext() => false;

        [Category("Data")]
        [ParenthesizePropertyName(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new ControlBindingsCollection DataBindings
        {
            get
            {
                _dataBindings ??= new ControlBindingsCollection(this);
                return _dataBindings;
            }
        }
    }
}
