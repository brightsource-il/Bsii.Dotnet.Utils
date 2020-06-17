using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Bsii.Dotnet.Utils.Collections;
using Xunit;

namespace Bsii.Dotnet.Utils.Tests
{
    // TODO: Extensions to test
    // done - this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory
    // done - this Dictionary<TKey, TValue> dictionary, TKey addKey, TValue addValue, TValue updateValue
    // done - this Dictionary<TKey, TValue> dictionary, TKey addKey, Func<TKey, TValue> addValueFactory, TValue updateValue
    // done - this Dictionary<TKey, TValue> dictionary, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory
    // done - this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Action<TKey, TValue> updateAction
    // done - this Dictionary<TKey, TValue> dictionary, TKey addKey, TValue addValue, Action<TKey, TValue> updateAction
    // this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Task<TValue>> addValueFactory, Func<TKey, TValue, Task> updateValueFactory

    public class DictionaryExtensionsTest
    {
        class TestClass
        {
            public int A { get; set; }
        }

        struct TestStruct
        {
            public int A { get; set; }
        }

        private void TestClassUpdateAction(int key, TestClass value) => value.A = 3;


        [Fact]
        public void TestBasicInsertionPrimitive()
        {
            var primitivesMap = new Dictionary<int, int>();
            primitivesMap.AddOrUpdate(1, 2, 3);
            Assert.Contains(1, primitivesMap.Keys);
            Assert.Equal(2, primitivesMap[1]);
        }

        [Fact]
        public void TestBasicInsertionStruct()
        {
            var structsMap = new Dictionary<int, TestStruct>();
            var addValueStruct = new TestStruct {A = 2};
            var valueToAddStruct = new TestStruct {A = 3};
            structsMap.AddOrUpdate(1, addValueStruct, valueToAddStruct);
            Assert.Contains(1, structsMap.Keys);
            Assert.Equal(addValueStruct.A, structsMap[1].A);
            addValueStruct.A = 5;
            Assert.Equal(2, structsMap[1].A);
        }

        [Fact]
        public void TestBasicInsertionClass()
        {
            var classMap = new Dictionary<int, TestClass>();
            var valueToAddClass = new TestClass {A = 2};
            var valueToUpdateClass = new TestClass {A = 3};
            classMap.AddOrUpdate(1, valueToAddClass, valueToUpdateClass);
            Assert.Contains(1, classMap.Keys);
            Assert.Equal(valueToAddClass, classMap[1]);
            Assert.NotEqual(valueToUpdateClass, classMap[1]);
            valueToAddClass.A = 5;
            Assert.Equal(5, classMap[1].A);
        }

        [Fact]
        public void TestBasicUpdatePrimitive()
        {
            var primitiveMap = new Dictionary<int, int> {[1] = 1};
            primitiveMap.AddOrUpdate(1, 1, 2);
            Assert.Contains(1, primitiveMap.Keys);
            Assert.Equal(2, primitiveMap[1]);
        }

        [Fact]
        public void TestBasicUpdateStruct()
        {
            var structsMap = new Dictionary<int, TestStruct> {[1] = new TestStruct {A = 2}};
            var addValueStruct = new TestStruct {A = 2};
            var valueToAddStruct = new TestStruct {A = 3};
            structsMap.AddOrUpdate(1, addValueStruct, valueToAddStruct);
            Assert.Contains(1, structsMap.Keys);
            Assert.Equal(valueToAddStruct.A, structsMap[1].A);
            valueToAddStruct.A = 5;
            Assert.Equal(3, structsMap[1].A);
        }

        [Fact]
        public void TestBasicUpdateClass()
        {
            var classMap = new Dictionary<int, TestClass> {[1] = new TestClass {A = 2}};
            var valueToAddClass = new TestClass {A = 2};
            var valueToUpdateClass = new TestClass {A = 3};
            classMap.AddOrUpdate(1, valueToAddClass, valueToUpdateClass);
            Assert.Contains(1, classMap.Keys);
            Assert.Equal(valueToUpdateClass, classMap[1]);
            Assert.NotEqual(valueToAddClass, classMap[1]);
            valueToUpdateClass.A = 5;
            Assert.Equal(5, classMap[1].A);
        }

        // AddValue = Factory
        // UpdateValue = Factory
        [Fact]
        public void TestAddFactoryUpdateFactoryInsertion()
        {
            var dict = new Dictionary<int, int>();
            Func<int, int> addValueFactory = (key) => 2;
            Func<int, int, int> updateValueFactory = (key, value) => 3;
            dict.AddOrUpdate(1, addValueFactory, updateValueFactory);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 2);
        }

        [Fact]
        public void TestAddFactoryUpdateFactoryUpdate()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            Func<int, int> addValueFactory = (key) => 2;
            Func<int, int, int> updateValueFactory = (key, value) => 3;
            dict.AddOrUpdate(1, addValueFactory, updateValueFactory);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 3);
        }

        // AddValue = Factory
        // UpdateValue = TValue
        [Fact]
        public void TestAddFactoryInsertion()
        {
            var dict = new Dictionary<int, int>();
            Func<int, int> addValueFactory = (key) => 2;
            dict.AddOrUpdate(1, addValueFactory, 3);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 2);
        }

        [Fact]
        public void TestAddFactoryUpdate()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            Func<int, int> addValueFactory = (key) => 2;
            dict.AddOrUpdate(1, addValueFactory, 3);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 3);
        }

        // AddValue = TValue
        // UpdateValue = Factory
        [Fact]
        public void TestUpdateFactoryInsertion()
        {
            var dict = new Dictionary<int, int>();
            Func<int, int, int> updateValueFactory = (key, value) => 3;
            dict.AddOrUpdate(1, 2, updateValueFactory);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 2);
        }

        [Fact]
        public void TestUpdateFactoryUpdate()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            Func<int, int, int> updateValueFactory = (key, value) => 3;
            dict.AddOrUpdate(1, 2, updateValueFactory);
            Assert.True(dict.ContainsKey(1));
            Assert.True(dict[1] == 3);
        }

        // AddValue = Factory
        // UpdateValue = GenericAction
        [Fact]
        public void TestAddFactoryGenericActionPrimitiveInsertion()
        {
            var dict = new Dictionary<int, int>();
            Func<int, int> addValueFactory = (key) => 2;
            var updatedFlag = false;
            dict.AddOrUpdate(1, addValueFactory, (_, __) => updatedFlag = true);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
            Assert.False(updatedFlag);
        }

        [Fact]
        public void TestAddFactoryGenericActionPrimitiveUpdate()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            Func<int, int> addValueFactory = (key) => 2;
            var updatedFlag = false;
            dict.AddOrUpdate(1, addValueFactory, (_, __) => updatedFlag = true);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
            Assert.True(updatedFlag);
        }

        [Fact]
        public void TestAddFactoryGenericActionClassInsertion()
        {
            var addValue = new TestClass {A = 2};
            var dict = new Dictionary<int, TestClass>();
            Func<int, TestClass> addValueFactory = (key) => addValue;
            dict.AddOrUpdate(1, addValueFactory, TestClassUpdateAction);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1].A);
        }

        [Fact]
        public void TestAddFactoryGenericActionClassUpdate()
        {
            var addValue = new TestClass {A = 2};
            var dict = new Dictionary<int, TestClass> {[1] = addValue};
            Func<int, TestClass> addValueFactory = (key) => addValue;
            dict.AddOrUpdate(1, addValueFactory, TestClassUpdateAction);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(3, dict[1].A);
        }


        // AddValue = TValue
        // UpdateValue = GenericAction
        [Fact]
        public void TestAddTValueGenericActionPrimitiveInsertion()
        {
            var dict = new Dictionary<int, int>();
            var updatedFlag = false;
            dict.AddOrUpdate(1, 2, (_, __) => updatedFlag = true);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
            Assert.False(updatedFlag);
        }

        [Fact]
        public void TestAddTValueGenericActionPrimitiveUpdate()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            var updatedFlag = false;
            dict.AddOrUpdate(1, 2, (_, __) => updatedFlag = true);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
            Assert.True(updatedFlag);
        }

        [Fact]
        public void TestAddTValueGenericActionClassInsertion()
        {
            var addValue = new TestClass {A = 2};
            var dict = new Dictionary<int, TestClass>();
            dict.AddOrUpdate(1, addValue, TestClassUpdateAction);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1].A);
        }

        [Fact]
        public void TestAddTValueGenericActionClassUpdate()
        {
            var addValue = new TestClass {A = 2};
            var dict = new Dictionary<int, TestClass> {[1] = addValue};
            dict.AddOrUpdate(1, addValue, TestClassUpdateAction);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(3, dict[1].A);
        }

        // AddValue = Async Factory
        // UpdateValue = Async Factory
        [Fact]
        public async void TestAddFactoryUpdateFactoryInsertionAsync()
        {
            var dict = new Dictionary<int, int>();
            Func<int, Task<int>> asyncAddFunc = (key) => Task.FromResult(2);
            var flag = false;
            Func<int, int, Task> asyncUpdateFunc = (key, value) => { return Task.Run(() => flag = true); };
            await dict.AddOrUpdateAsync(1, asyncAddFunc, asyncUpdateFunc);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
            Assert.False(flag);
        }

        [Fact]
        public async void TestAddFactoryUpdateFactoryUpdateAsync()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            Func<int, Task<int>> asyncAddFunc = (key) => { return new Task<int>(() => 2); };
            var flag = false;
            Func<int, int, Task> asyncUpdateFunc = (key, value) => { return Task.Run(() => flag = true); };
            await dict.AddOrUpdateAsync(1, asyncAddFunc, asyncUpdateFunc);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
            Assert.True(flag);
        }

        [Fact]
        public void TestGetOrAddTValueInsertion()
        {
            var dict = new Dictionary<int, int> { };
            dict.GetOrAdd(1, 2);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
        }

        [Fact]
        public void TestGetOrAddTValueGet()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            dict.GetOrAdd(1, 3);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
        }

        [Fact]
        public void TestGetOrAddFactoryTValueInsertion()
        {
            var dict = new Dictionary<int, int> { };
            Func<int, int> addValueFactory = (key) => 2;
            dict.GetOrAdd(1, addValueFactory);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
        }

        [Fact]
        public void TestGetOrAddFactoryTValueGet()
        {
            var dict = new Dictionary<int, int> {[1] = 2};
            Func<int, int> addValueFactory = (key) => 2;
            dict.GetOrAdd(1, addValueFactory);
            Assert.Contains(1, dict.Keys);
            Assert.Equal(2, dict[1]);
        }
    }
}