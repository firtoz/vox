using System;
using System.Collections.Generic;
using OctreeTest.UnitTestRunner;
using UnityEngine;

namespace OctreeTest
{
    public interface IUnitTestEngine
    {
        UnitTestRendererLine GetTests(out UnitTestResult[] results, out string[] categories);
        void RunTests(TestFilter filter, ITestRunnerCallback testRunnerEventListener);
    }
}
