using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AlanAamy.Net.RxTimedWindow
{
    public static class SampleExtentions
    {
        public static void Dump<T>(this IObservable<T> source, string name)
        {
            source.Subscribe(
            i => Console.WriteLine("{0}-->{1}", name, i),
            ex => Console.WriteLine("{0} failed-->{1}", name, ex.Message),
            () => Console.WriteLine("{0} completed", name));
        }

public static IObservable<TResult> FromTdf<T, TResult>(
    this IObservable<T> source,
    IPropagatorBlock<T, TResult> block)
{
    return Observable.Defer<TResult>(() =>
        {
            source.Catch((Exception ex) => Observable.Throw<T>(ex))
                  .Subscribe(block.AsObserver());
            return block.AsObservable();
        });
}

        public static IObservable<TResult> FromTdf<T, TResult>(
            this IObservable<T> source,
            Func<IPropagatorBlock<T, TResult>> blockFactory)
        {
            return Observable.Defer<TResult>(() =>
            {
                var block = blockFactory();
                source.Subscribe(block.AsObserver());
                return block.AsObservable();
            });
        }
    }
}
