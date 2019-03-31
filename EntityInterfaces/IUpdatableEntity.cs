using System;
using System.Collections.Generic;
using System.Text;
using CommonBasicStandardLibraries.CollectionClasses;
namespace DapperHelpersLibrary.EntityInterfaces
{
    public interface IUpdatableEntity : ISimpleDapperEntity
    {
        void Initialize(); //if i initialize before reaching client, then it would have to send the dictionary to the client.
        //Dictionary<string, object> GetChanges(); //getchanges will usually be called from the sql server.
        CustomBasicList<string> GetChanges(); //only needs to know the fields that changed.
    }
}