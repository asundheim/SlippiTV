using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlippiTV.Shared.Types;

[JsonObject]
public class UpdateInfo
{
    public required string UpdateScript { get; set; }
    public required string UpdateLink { get; set; }
    public required string UpdateFileName { get; set; }
}
