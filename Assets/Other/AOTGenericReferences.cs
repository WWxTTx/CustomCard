using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"UniTask.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GameFramework.Runtime.EventManager.<PublishInBackground>d__10<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GameFramework.Runtime.EventManager.<PublishInBackground>d__10<object>>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// System.Action<GameFramework.Runtime.PlayerInputEventDefine.CardDrawn>
	// System.Action<GameFramework.Runtime.PlayerInputEventDefine.CardSelected>
	// System.Action<int>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Func<int>
	// System.Func<object>
	// System.Predicate<int>
	// System.Predicate<object>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.SwitchToMainThreadAwaitable.Awaiter,GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>>(Cysharp.Threading.Tasks.SwitchToMainThreadAwaitable.Awaiter&,GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.SwitchToThreadPoolAwaitable.Awaiter,GameFramework.Runtime.EventManager.<PublishInBackground>d__10<object>>(Cysharp.Threading.Tasks.SwitchToThreadPoolAwaitable.Awaiter&,GameFramework.Runtime.EventManager.<PublishInBackground>d__10<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GameFramework.Runtime.EventManager.<PublishInBackground>d__10<object>>(GameFramework.Runtime.EventManager.<PublishInBackground>d__10<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>>(GameFramework.Runtime.EventManager.<PublishInNextFrame>d__11<object>&)
	}
}