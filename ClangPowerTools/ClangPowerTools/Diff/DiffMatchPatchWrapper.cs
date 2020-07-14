using Google.DiffMatchPatch;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class DiffMatchPatchWrapper
  {
    #region Members

    private readonly DiffMatchPatch diffMatchPatch;
    private List<Diff> diffs;

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

    /// <summary>
    /// An array of differences is computed which describe the transformation of text1 into text2. 
    /// Each difference is an array of Diff objects. The first element specifies if it is an insertion (1), 
    /// a deletion (-1) or an equality (0). The second element specifies the affected text.
    /// </summary>
    public void Diff(string text1, string text2)
    {
      diffs = diffMatchPatch.diff_main(text1, text2);
    }

    /// <summary>
    /// A diff of two unrelated texts can be filled with coincidental matches. 
    /// For example, the diff of "mouse" and "sofas" is [(-1, "m"), (1, "s"), (0, "o"), (-1, "u"), (1, "fa"), (0, "s"), (-1, "e")]. 
    /// While this is the optimum diff, it is difficult for humans to understand. Semantic cleanup rewrites the diff, expanding it into a more intelligible format. 
    /// The above example would become: [(-1, "mouse"), (1, "sofas")]. If a diff is to be human-readable, it should be passed to diff_cleanupSemantic
    /// </summary>
    public void CleanupSemantic()
    {
      if (diffs == null) return;
      diffMatchPatch.diff_cleanupSemantic(diffs);
    }

    /// <summary>
    /// Given a diff, measure its Levenshtein distance in terms of the number of inserted, deleted or substituted characters. 
    /// The minimum distance is 0 which means equality, the maximum distance is the length of the longer string.
    /// </summary>
    /// <returns>It returns -1 if the diffs are null</returns>
    public int DiffLevenshtein()
    {
      if (diffs == null) return -1;
      return diffMatchPatch.diff_levenshtein(diffs);
    }


    #endregion

  }
}
