using FileUpdate.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUpdate.Client
{
    public class Difference
    {
        public Difference(IList<Asset> deleteds, IList<Asset> modifieds)
        {
            ArgumentNullException.ThrowIfNull(deleteds, nameof(deleteds));
            ArgumentNullException.ThrowIfNull(modifieds, nameof(modifieds));

            Deleteds = new(deleteds);
            Modifieds = new(modifieds);
        }

        public ReadOnlyCollection<Asset> Deleteds { get; }

        public ReadOnlyCollection<Asset> Modifieds { get;}
    }
}
