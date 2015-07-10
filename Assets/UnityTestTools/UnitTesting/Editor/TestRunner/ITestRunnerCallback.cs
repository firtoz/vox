using System;
using System.Collections.Generic;
using UnityEngine;

namespace OctreeTest.UnitTestRunner
{
    public interface ITestRunnerCallback
    {
        void TestStarted(string fullName);
        void TestFinished(ITestResult fullName);
        void RunStarted(string suiteName, int testCount);
        void RunFinished();
        void RunFinishedException(Exception exception);
    }
}
