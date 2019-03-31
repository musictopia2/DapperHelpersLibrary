using System;
using System.Collections.Generic;
using System.Text;

namespace DapperHelpersLibrary.EntityInterfaces
{
    public interface IJoinedEntity : ISimpleDapperEntity //hopefully i don't need more than 3 joined.  if so, rethink
    {
        void AddRelationships<E>(E Entity);
    }
    public interface IJoin3Entity<D1, D2> : ISimpleDapperEntity
    {
        void AddRelationships(D1 FirstEntity, D2 SecondEntity);
    }
}
