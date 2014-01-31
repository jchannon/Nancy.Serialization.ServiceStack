namespace Nancy.Serialization.ServiceStack.Tests
{
    using System;
    using System.Collections.Generic;
    using Nancy.ModelBinding;
    using Nancy.Testing;
    using Xunit;
    using Nancy.Serializers.Json.ServiceStack;

    public class ModelBindingFixture
    {
        [Fact]
        public void Should_BindTo_Existing_Instance_Using_Body_Serializer()
        {
            //Given
            var module = new ConfigurableNancyModule(c => c.Post("/instance", (_, m) =>
            {
                var model = new Stuff() { Id = 1 };
                m.BindTo(model);
                return model;
            }));

            var bootstrapper = new TestBootstrapper(config => config.Module(module));

            var postmodel = new Stuff { Name = "Marsellus Wallace" };

            var browser = new Browser(bootstrapper);

            //When
            var result = browser.Post("/instance", with =>
            {
                with.JsonBody(postmodel, new ServiceStackJsonSerializer());
                with.Accept("application/json");
            });

            var resultModel = result.Body.DeserializeJson<Stuff>();

            //Then
            Assert.Equal("Marsellus Wallace", resultModel.Name);
            Assert.Equal(1, resultModel.Id);
        }

        [Fact]
        public void Should_BindTo_Existing_Instance_Using_Body_Serializer_And_BlackList()
        {
            //Given
            var module = new ConfigurableNancyModule(c => c.Post("/instance", (_, m) =>
            {
                var model = new Stuff() { Id = 1 };
                m.BindTo(model, new[] { "LastName" });
                return model;
            }));

            var bootstrapper = new TestBootstrapper(config => config.Module(module));

            var postmodel = new Stuff { Name = "Marsellus Wallace", LastName = "Smith" };

            var browser = new Browser(bootstrapper);

            //When
            var result = browser.Post("/instance", with =>
            {
                with.JsonBody(postmodel, new ServiceStackJsonSerializer());
                with.Accept("application/json");
            });

            var resultModel = result.Body.DeserializeJson<Stuff>();

            //Then
            Assert.Null(resultModel.LastName);
        }
    }
    public class TestBootstrapper : ConfigurableBootstrapper
    {
        public TestBootstrapper(Action<ConfigurableBootstrapperConfigurator> configuration)
            : base(configuration)
        {
        }

        public TestBootstrapper()
        {
        }

        protected override IEnumerable<Type> BodyDeserializers
        {
            get
            {
                yield return typeof(ServiceStackBodyDeserializer);
            }
        }
    }

    public class Stuff
    {
        public Stuff()
        {
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public Stuff(int id)
        {
            Id = id;
        }
    }
}
