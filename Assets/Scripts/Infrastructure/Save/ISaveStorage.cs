public interface ISaveStorage
{
    bool Exists();
    bool TryRead(out string content);
    bool TryWrite(string content);
    bool TryDelete();
}