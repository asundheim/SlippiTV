using System.Runtime.InteropServices;

namespace SlippiTV.Client.Platforms.Windows.RustInvoker;

[StructLayout(LayoutKind.Sequential)]
internal struct SlippiRustRankInfo
{
    public int FetchStatus;
    public SlippiRustRank Rank;
    public float RatingOrdinal;
    public uint RatingUpdateCount;
    public float RatingChange;
    public int RankChange;
}
