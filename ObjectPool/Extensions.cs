namespace ObjectPool
{
    internal static class Extensions
    {
        public static CancellationToken CancelAfter(this CancellationToken cancellationToken, int millisecondsDelay)
        {
            var waitingCancelationTokenSource = new CancellationTokenSource();
            waitingCancelationTokenSource.CancelAfter(millisecondsDelay);
            var waitingCancellationToken = waitingCancelationTokenSource.Token;
            return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, waitingCancellationToken).Token;
        }
    }
}
