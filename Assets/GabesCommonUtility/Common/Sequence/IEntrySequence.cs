#if UNITASK

using System;
using Cysharp.Threading.Tasks;

namespace GabesCommonUtility.Sequence
{
   public interface IEntrySequence
   {
      public event Action<string> DisplayMessage;
      public UniTask<IEntrySequence> ExecuteSequence();
      public IEntrySequence Default { get; }
      public bool IsCompleted { get; }
   }
}

#endif