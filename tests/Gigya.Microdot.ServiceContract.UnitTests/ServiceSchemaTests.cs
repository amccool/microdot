﻿#region Copyright 
// Copyright 2017 Gigya Inc.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Threading.Tasks;
using Gigya.Common.Contracts.HttpService;
using Newtonsoft.Json;
using NUnit.Framework;
#pragma warning disable 169

namespace Gigya.Common.Contracts.UnitTests
{

    class Data
    {
        string s;
        Nested n;
    }

    class Nested
    {
        DateTime time;
    }

    public class SensitiveAttribute : Attribute {}

    [HttpService(100, Name = "ServiceName")]
    internal interface ITestInterface
    {
        [PublicEndpoint("demo.doSomething")]
        Task DoSomething(int i, double? nd, string s, [Sensitive] Data data);
    }

    [TestFixture]
    public class ServiceSchemaTests
    {

        [Test]
        public void TestSerialization()
        {
            ServiceSchema schema = new ServiceSchema(new[] { typeof(ITestInterface) });
            string serialized = JsonConvert.SerializeObject(schema, Formatting.Indented);
            schema = JsonConvert.DeserializeObject<ServiceSchema>(serialized);

            Assert.IsTrue(schema.Interfaces.Length == 1);
            Assert.IsTrue(schema.Interfaces[0].Name == typeof(ITestInterface).FullName);
            Assert.IsTrue(schema.Interfaces[0].Attributes.Length == 1);
            Assert.IsTrue(schema.Interfaces[0].Attributes[0].Attribute is HttpServiceAttribute);
            Assert.IsTrue(schema.Interfaces[0].Attributes[0].TypeName == typeof(HttpServiceAttribute).AssemblyQualifiedName);
            Assert.IsTrue((schema.Interfaces[0].Attributes[0].Attribute as HttpServiceAttribute).BasePort == 100);
            Assert.IsTrue((schema.Interfaces[0].Attributes[0].Attribute as HttpServiceAttribute).Name == "ServiceName");
            Assert.IsTrue(schema.Interfaces[0].Methods.Length == 1);
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Name == nameof(ITestInterface.DoSomething));
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Attributes.Length == 1);
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Attributes[0].Attribute is PublicEndpointAttribute);
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Attributes[0].TypeName == typeof(PublicEndpointAttribute).AssemblyQualifiedName);
            Assert.IsTrue((schema.Interfaces[0].Methods[0].Attributes[0].Attribute as PublicEndpointAttribute).EndpointName == "demo.doSomething");
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters.Length == 4);
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[0].Attributes.Length == 0);
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[0].Name == "i");
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[0].Type == typeof(int));
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[0].TypeName == typeof(int).AssemblyQualifiedName);
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[1].Name == "nd");
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[1].Type == typeof(double?));
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[2].Name == "s");
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[3].Attributes.Length == 1);
            Assert.IsTrue(schema.Interfaces[0].Methods[0].Parameters[3].Attributes[0].Attribute is SensitiveAttribute);
        }


        [Test]
        public void TestUnknownAttribute()
        {
            var typeFullName = typeof(SensitiveAttribute).AssemblyQualifiedName;
            string json = @"
                {
                  ""TypeName"": """ + typeFullName + @""",
                  ""Data"": {
                    ""TypeId"": """ + typeFullName + @"""
                  }
                }";
            AttributeSchema attr = JsonConvert.DeserializeObject<AttributeSchema>(json);
            Assert.IsNotNull(attr.Attribute);
        
            json = @"
                {
                  ""TypeName"": ""Gigya.Microdot.ServiceContract.UnitTests.HttpService.SensitiveAttribute2, Gigya.Microdot.ServiceContract.UnitTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",
                  ""Data"": {
                    ""TypeId"": """ + typeFullName + @"""
                  }
                }";
            attr = JsonConvert.DeserializeObject<AttributeSchema>(json);
            Assert.IsNull(attr.Attribute);
            Assert.IsTrue(attr.TypeName == "Gigya.Microdot.ServiceContract.UnitTests.HttpService.SensitiveAttribute2, Gigya.Microdot.ServiceContract.UnitTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        }


        [Test]
        public void TestUnknownParamType()
        {
            string json = @"
                {
                  ""Name"": ""i"",
                  ""TypeName"": ""System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"",
                  ""Attributes"": []
                }";
            ParameterSchema param = JsonConvert.DeserializeObject<ParameterSchema>(json);
            Assert.IsNotNull(param.Type);

            json = @"
                {
                  ""Name"": ""i"",
                  ""TypeName"": ""System.Int33, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"",
                  ""Attributes"": []
                }";
            param = JsonConvert.DeserializeObject<ParameterSchema>(json);
            Assert.IsNull(param.Type);
            Assert.IsTrue(param.TypeName == "System.Int33, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }
    }
}
