using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDrop
{
    class TextMessage : INotifyPropertyChanged
    {
        private string _messageText;

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                _messageText = value;
                this.OnPropertyChanged("MessageText");
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        ///     Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members

        #region Debugging Aides

        /// <summary>
        ///     Warns the developer if this object does not have
        ///     a public property with the specified name. This
        ///     method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                var msg = "Invalid property name: " + propertyName;

                if (ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                Debug.Fail(msg);
            }
        }

        /// <summary>
        ///     Returns whether an exception is thrown, or if a Debug.Fail() is used
        ///     when an invalid property name is passed to the VerifyPropertyName method.
        ///     The default value is false, but subclasses used by unit tests might
        ///     override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion Debugging Aides
    }
}
