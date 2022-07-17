using System;
using System.Threading.Tasks;

namespace FetaWarrior.Extensions;

// Use a source generator for these lovely extensions
// And probably include those in a separate package instead of Garyon
public static class TaskReturningDelegateExtensions
{
    public static Action<T> WrapSync<T>(this Func<T, Task> taskFunc)
    {
        return taskFunc.AsWrapped().PerformSync;
    }
    public static Func<T1, TResult> WrapSync<T1, TResult>(this Func<T1, Task<TResult>> taskFunc)
    {
        return taskFunc.AsWrapped().PerformSync;
    }

    public static WrappedVoidTaskFunc<T> AsWrapped<T>(this Func<T, Task> taskFunc)
    {
        return new WrappedVoidTaskFunc<T>(taskFunc);
    }
    public static WrappedTaskFunc<T1, TResult> AsWrapped<T1, TResult>(this Func<T1, Task<TResult>> taskFunc)
    {
        return new WrappedTaskFunc<T1, TResult>(taskFunc);
    }

    public static async Task AwaitInvokeNullSafe<T>(this Func<T, Task> awaitable, T arg)
    {
        if (awaitable is not null)
            await awaitable(arg);
    }

    public static async Task<TReturn> AwaitInvokeNullSafe<T, TReturn>(this Func<T, Task<TReturn>> awaitable, T arg)
    {
        if (awaitable is not null)
            return await awaitable(arg);

        return default;
    }
}

public struct WrappedVoidTaskFunc<T>
{
    private readonly Func<T, Task> func;

    public WrappedVoidTaskFunc(Func<T, Task> taskFunc)
    {
        func = taskFunc;
    }

    public void PerformSync(T arg)
    {
        func(arg).Wait();
    }
}

public struct WrappedTaskFunc<T1, TResult>
{
    private readonly Func<T1, Task<TResult>> func;

    public WrappedTaskFunc(Func<T1, Task<TResult>> taskFunc)
    {
        func = taskFunc;
    }

    public TResult PerformSync(T1 arg)
    {
        return func(arg).Result;
    }
}
