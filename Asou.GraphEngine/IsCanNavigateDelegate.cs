using Asou.Abstractions.ExecutionElements;
using Asou.Abstractions.Process;

namespace Asou.GraphEngine;

public delegate bool IsCanNavigateDelegate(IProcessInstance processInstance, BaseElement from, BaseElement to);
