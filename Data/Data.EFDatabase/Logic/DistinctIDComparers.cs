using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Data.Model.EFDbModel;

namespace Data.EFDatabase.Logic {
    public class NodesComparer : IEqualityComparer<NtwkNode> {
        public bool Equals(NtwkNode x, NtwkNode y) {
            return x.ID.Equals(y.ID);
        }

        public int GetHashCode(NtwkNode obj) {
            return obj.ID.GetHashCode();
        }
    }
}