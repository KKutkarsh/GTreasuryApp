using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTreasury.Api.Utilities.Dtos
{
    public record NpvSettings
    {
        public int MaxBatchSize { get; init; }
    }
}
