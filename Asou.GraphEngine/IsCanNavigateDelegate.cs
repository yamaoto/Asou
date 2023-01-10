using Asou.Abstractions.ExecutionElements;
using Asou.Core;

namespace Asou.GraphEngine;

public delegate bool IsCanNavigateDelegate(IProcessInstance processInstance, BaseElement from, BaseElement to);