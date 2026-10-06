public struct RimPlacement
{
    public readonly int FrontX;
    public readonly int FrontY;
    public readonly int Dx;
    public readonly int Dy;
    public readonly int EndX;
    public readonly int EndY;
    public readonly int Score;

    public readonly int Glue;

    public RimPlacement(int frontX, int frontY, int dx, int dy, int endX, int endY, int score, int glue)
    {
        FrontX = frontX;
        FrontY = frontY;
        Dx = dx;
        Dy = dy;
        EndX = endX;
        EndY = endY;
        Score = score;
        Glue = glue;
    }
}