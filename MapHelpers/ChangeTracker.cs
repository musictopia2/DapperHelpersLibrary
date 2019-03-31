using System;
using System.Text;
using CommonBasicStandardLibraries.Exceptions;
using CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;
using System.Linq;
using CommonBasicStandardLibraries.BasicDataSettingsAndProcesses;
using static CommonBasicStandardLibraries.BasicDataSettingsAndProcesses.BasicDataFunctions;
using CommonBasicStandardLibraries.CollectionClasses;
using System.Threading.Tasks; //most of the time, i will be using asyncs.
using fs = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.FileHelpers;
using js = CommonBasicStandardLibraries.AdvancedGeneralFunctionsAndProcesses.JsonSerializers.NewtonJsonStrings; //just in case i need those 2.
using DapperHelpersLibrary.Extensions;
using System.Reflection;
using DapperHelpersLibrary.ConditionClasses;
using DapperHelpersLibrary.Attributes;//i think this is the most common things i like to do
using System.Collections.Generic;
namespace DapperHelpersLibrary.MapHelpers
{
    public class ChangeTracker
    {
        private Dictionary<string, object> OriginalValues = new Dictionary<string, object>();

        public void PopulateOriginalDictionary(Dictionary<string, object> SavedOriginal) //the server has to put in the original dictionary
        {
            OriginalValues = SavedOriginal;
        }

        public Dictionary<string, object> GetOriginalValues()
        {
            return new Dictionary<string, object>(OriginalValues);
        }
        
        public void Initialize() //from api if updating, needs to call the populateoriginaldictionary
        {
            //you are on your own if you mess up unfortunately.
            OriginalValues.Clear();
            var TempList = ThisType.GetProperties().Where(Items => Items.CanMapToDatabase() == true && Items.Name!="ID"); //id can never be tracked
            TempList = TempList.Where(Items => Items.HasAttribute<ExcludeUpdateListenerAttribute>() == false);
            TempList = TempList.Where(Items => Items.HasAttribute<ForeignKeyAttribute>() == false); //because the foreigns would never be updated obviously.
            foreach (PropertyInfo property in TempList)
            {
                OriginalValues.Add(property.Name, property.GetValue(ThisObject, null));
            }
        }
        //one problem though.
        //after i do one update, then if i do another, its going to think it updated again even though already reflected.

        //looks like after a client sends to server, it has to initialize again.
        //only if successful.
        //obviously if they failed to send to server, no need.

        private readonly object ThisObject;
        private readonly Type ThisType;

        public ChangeTracker(object _Object)
        {
            ThisObject = _Object;
            ThisType = ThisObject.GetType();
        }
        public CustomBasicList<string> GetChanges()
        {
            CustomBasicList<string> output = new CustomBasicList<string>();
            foreach(var ThisValue in OriginalValues)
            {
                PropertyInfo property = ThisType.GetProperties().Where(Items => Items.Name == ThisValue.Key).Single();
                object NewValue = property.GetValue(ThisObject, null);
                if (IsUpdate(ThisValue.Value, NewValue) == true)
                    output.Add(property.Name);
            }
            return output;
        }

        private bool IsUpdate(object ThisValue, object NewValue)
        {
            if (ThisValue == null && NewValue == null)
                return false;
            if (ThisValue == null)
                return true;
            if (ThisValue.Equals(NewValue) == false)
                return true;
            return false;
        }

    }
}
