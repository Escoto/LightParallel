using System.Collections.Generic;

namespace LightParallelTests.Mocks
{
    public class MockInput
    {
        public bool WasProcessed;

        public MockInput()
        {
            WasProcessed = false;
        }
    }

    public class MockInputList
    {
        public readonly IEnumerable<MockInput> Inputs = new List<MockInput>()
        {
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput(),
            new MockInput()
        };
    }
}
