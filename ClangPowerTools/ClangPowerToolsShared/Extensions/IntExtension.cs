namespace ClangPowerTools
{
  public static class IntExtension
  {
    public static int ForceInRange(this int number, int min, int max)
    {
      if (number < min)
        number = min;

      if (number > max)
        number = max;

      return number;
    }

    public static int RoundUp(this int number)
    {
      return 10 * ((number + 9) / 10);
    }

  }
}
