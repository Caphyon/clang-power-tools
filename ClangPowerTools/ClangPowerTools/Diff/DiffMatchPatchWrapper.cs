using Google.DiffMatchPatch;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class DiffMatchPatchWrapper
  {
    #region Members

    private readonly DiffMatchPatch diffMatchPatch;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize DiffMatchPatch and use the default values
    /// </summary>
    public DiffMatchPatchWrapper()
    {
      diffMatchPatch = new DiffMatchPatch();
    }

    /// <summary>
    /// Initialize DiffMatchPatch and overwrite the default values
    /// </summary>
    /// <param name="diffTimeout">Number of seconds to map a diff before giving up (0 for infinity).</param>
    /// <param name="diffEditCost">Cost of an empty edit operation in terms of edit characters.</param>
    /// <param name="matchThreshold">At what point is no match declared (0.0 = perfection, 1.0 = very loose).</param>
    /// <param name="matchDistance">How far to search for a match (0 = exact location, 1000+ = broad match). <br/>
    /// A match this many characters away from the expected location will add 1.0 to the score (0.0 is a perfect match)</param>
    /// <param name="patchDeleteThreshold">When deleting a large block of text (over ~64 characters), how close do the contents have to be to match the expected contents.<br/> 
    /// (0.0 = perfection, 1.0 = very loose).  Note that Match_Threshold controls how closely the end points of a delete need to match.</param>
    /// <param name="patchMargin">Chunk size for context length.</param>
    public DiffMatchPatchWrapper(float diffTimeout, short diffEditCost, float matchThreshold, int matchDistance, float patchDeleteThreshold, short patchMargin)
    {
      diffMatchPatch = new DiffMatchPatch
      {
        Diff_Timeout = diffTimeout,
        Diff_EditCost = diffEditCost,
        Match_Threshold = matchThreshold,
        Match_Distance = matchDistance,
        Patch_DeleteThreshold = patchDeleteThreshold,
        Patch_Margin = patchMargin
      };
    }

    #endregion

    #region Methods


    public List<Diff> Diff(string text1, string text2)
    {
      return diffMatchPatch.diff_main(text1, text2);
    }

    #endregion

  }
}
