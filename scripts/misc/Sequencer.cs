public class Sequencer<T>
{
    private int current;
    private T[] sequence;
    public bool isFinished => current == sequence.Length - 1;
    public bool isStarted => current > 0;

    public Sequencer(T[] sequence){
        this.sequence = sequence;
        current = 0;
    }

    /// <summary>
    /// Returns the next element in the sequence, or the last element if already at the end.
    /// </summary>
    /// <returns>The next element in the sequence.</returns>
    public T Next(){
        if (current < sequence.Length - 1)
            current++;
        return sequence[current];
    }

    /// <summary>
    /// Returns the previous element in the sequence, or the first element if already at the start.
    /// </summary>
    /// <returns>The previous element in the sequence.</returns>
    public T Previous(){
        if (current > 0)
            current--;
        return sequence[current];
    }

    /// <summary>
    /// Returns the current element in the sequence.
    /// </summary>
    /// <returns>The current element in the sequence.</returns>
    public T Current(){
        return sequence[current];
    }
}
