namespace Installer.Core.Engine
{
    public sealed class RollbackStack
    {
        private readonly Stack<Func<Task>> _actions = new();
        public void Push(Func<Task> rollback) => _actions.Push(rollback);
        public async Task ExecuteAsync()
        {
            while (_actions.Count > 0)
            {
                var action = _actions.Pop();
                try { await action(); } catch { }
            }
        }
    }
}
