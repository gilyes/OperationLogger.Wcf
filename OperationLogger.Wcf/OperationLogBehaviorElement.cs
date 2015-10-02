using System;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Configuration;
using System.ServiceModel.Dispatcher;
using System.Text.RegularExpressions;

namespace OperationLogger.Wcf
{
    public class OperationLogBehaviorElement : BehaviorExtensionElement
    {
        private const string PatternsPropertyName = "patterns";

        public override Type BehaviorType
        {
            get { return typeof (OperationLogBehavior); }
        }

        [ConfigurationProperty(PatternsPropertyName)]
        [ConfigurationCollection(typeof (OperationLogPatternCollection),
            AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public OperationLogPatternCollection Patterns
        {
            get { return (OperationLogPatternCollection) base[PatternsPropertyName]; }
            set { base[PatternsPropertyName] = value; }
        }

        protected override object CreateBehavior()
        {
            return new OperationLogBehavior
            {
                IsEnabledForOperation = IsEnabledForOperation,
                IsParameterLoggingEnabled = IsParameterLoggingEnabled
            };
        }

        private bool IsEnabledForOperation(DispatchOperation dispatchOperation)
        {
            return GetPatternMatchingAction(dispatchOperation) != null;
        }

        private bool IsParameterLoggingEnabled(DispatchOperation dispatchOperation, string parameterName)
        {
            var actionMatch = GetPatternMatchingAction(dispatchOperation);
            return actionMatch != null && Regex.IsMatch(parameterName, actionMatch.ParameterPattern);
        }

        private OperationLogPatternElement GetPatternMatchingAction(DispatchOperation dispatchOperation)
        {
            return Patterns.OfType<OperationLogPatternElement>()
                .FirstOrDefault(x => Regex.IsMatch(dispatchOperation.Action, x.ActionPattern));
        }
    }

    public class OperationLogPatternCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public OperationLogPatternElement this[int index]
        {
            get { return (OperationLogPatternElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new OperationLogPatternElement this[string Name]
        {
            get { return (OperationLogPatternElement) BaseGet(Name); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new OperationLogPatternElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OperationLogPatternElement) element).ActionPattern;
        }

        public int IndexOf(OperationLogPatternElement patternElement)
        {
            return BaseIndexOf(patternElement);
        }

        public void Add(OperationLogPatternElement patternElement)
        {
            BaseAdd(patternElement);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(OperationLogPatternElement patternElement)
        {
            if (BaseIndexOf(patternElement) >= 0)
            {
                BaseRemove(patternElement.ActionPattern);
            }
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }
    }

    public class OperationLogPatternElement : ConfigurationElement
    {
        private const string ActionPropertyName = "actionPattern";
        private const string ParameterPatternPropertyName = "parameterPattern";

        [ConfigurationProperty(ActionPropertyName, DefaultValue = null, IsRequired = true, IsKey = false)]
        public string ActionPattern
        {
            get { return (string) this[ActionPropertyName]; }
            set { this[ActionPropertyName] = value; }
        }

        [ConfigurationProperty(ParameterPatternPropertyName, DefaultValue = ".*", IsRequired = false, IsKey = false)]
        public string ParameterPattern
        {
            get { return (string) this[ParameterPatternPropertyName]; }
            set { this[ParameterPatternPropertyName] = value; }
        }
    }
}