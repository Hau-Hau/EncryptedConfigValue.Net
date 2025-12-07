using EncryptedConfigValue.Crypto.Util;
using Shouldly;
using NSubstitute;
using System;
using Xunit;

namespace EncryptedConfigValue.Test.Util
{
    public class StringSubstitutionExceptionTest
    {
        private const string VALUE = "abc";

        private readonly Exception cause = Substitute.For<Exception>();

        [Fact]
        public void TestConstructions()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception, "");
        }

        [Fact]
        public void TestSingleExtendField()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception.Extend("field1"), "field1");
        }

        [Fact]
        public void TestDoubleExtendField()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception.Extend("field1").Extend("field2"), "field2.field1");
        }

        [Fact]
        public void TestSingleExtendArray()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception.Extend(1), "[1]");
        }

        [Fact]
        public void TestDoubleExtendArray()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception.Extend(1).Extend(2), "[2][1]");
        }

        [Fact]
        public void TestSingleExtendArrayAndField()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception.Extend(1).Extend("field"), "field[1]");
        }

        [Fact]
        public void TestSingleExtendArrayAndDoubleField()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception.Extend(1).Extend("field1").Extend("field2"), "field2.field1[1]");
        }

        [Fact]
        public void TestSingleExtendArrayBetweenDoubleField()
        {
            StringSubstitutionException exception = new StringSubstitutionException(cause, VALUE);
            AssertException(exception.Extend("field1").Extend(1).Extend("field2"), "field2[1].field1");
        }

        private void AssertException(StringSubstitutionException exception, String field)
        {
            exception.Value.ShouldBeEquivalentTo(VALUE);
            exception.Field.ShouldBeEquivalentTo(field);
            exception.Cause.ShouldBeEquivalentTo(cause);
        }
    }
}
