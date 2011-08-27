/*
Copyright (c) 2005 - 2010, Phil Haack
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

    *	Redistributions of source code must retain the above copyright notice, 
		this list of conditions and the following disclaimer.
    *	Redistributions in binary form must reproduce the above copyright notice, 
		this list of conditions and the following disclaimer in the documentation 
		and/or other materials provided with the distribution.
    *	Neither the name of the Subtext nor the names of its contributors 
		may be used to endorse or promote products derived from this software 
		without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY 
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetMigrations.Core.Data;
using NUnit.Framework;

namespace DotNetMigrations.UnitTests.Data
{
    [TestFixture]
    public class ScriptSplitterTests
    {
        [Test]
        public void CanParseCommentBeforeGoStatement()
        {
            const string script = @"SELECT FOO
/*TEST*/ GO
BAR";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(2, scripts.Count());
        }

        [Test]
        public void CanParseCommentWithQuoteChar()
        {
            const string script =
                @"/* Add the Url column to the subtext_Log table if it doesn't exist */
	ADD [Url] VARCHAR(255) NULL
GO
		AND		COLUMN_NAME = 'BlogGroup') IS NULL";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(2, scripts.Count());
        }

        [Test]
        public void CanParseDashDashCommentWithQuoteChar()
        {
            const string script =
                @"-- Add the Url column to the subtext_Log table if it doesn't exist
SELECT * FROM BLAH
GO
PRINT 'FOO'";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(2, scripts.Count());
        }

        [Test]
        public void CanParseGoWithDashDashCommentAfter()
        {
            const string script =
                @"SELECT * FROM foo;
 GO --  Hello Phil
CREATE PROCEDURE dbo.Test AS SELECT * FROM foo";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(2, scripts.Count());
        }

        [Test]
        public void CanParseLineEndingInDashDashComment()
        {
            const string script = @"SELECT * FROM BLAH -- Comment
GO
FOOBAR
GO";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(2, scripts.Count());
        }

        [Test]
        public void CanParseNestedComments()
        {
            const string script = @"/*
select 1
/* nested comment */
go
delete from users
-- */";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(1, scripts.Count(), "This contains a comment and no scripts.");
        }

        [Test]
        public void CanParseQuotedCorrectly()
        {
            const string script = @"INSERT INTO #Indexes
	EXEC sp_helpindex 'dbo.subtext_URLs'";

            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(script, scripts.First(), "Script text should not be modified");
        }

        [Test]
        public void CanParseSimpleScript()
        {
            string script = "Test" + Environment.NewLine + "go";
            List<string> scripts = new ScriptSplitter(script).ToList();
            Assert.AreEqual(1, scripts.Count());
            Assert.AreEqual("Test", scripts.First());
        }

        [Test]
        public void CanParseSimpleScriptEndingInNewLine()
        {
            string script = "Test" + Environment.NewLine + "GO" + Environment.NewLine;
            List<string> scripts = new ScriptSplitter(script).ToList();
            Assert.AreEqual(1, scripts.Count());
            Assert.AreEqual("Test", scripts.First());
        }

        [Test]
        public void CanParseSuccessiveGoStatements()
        {
            const string script = @"GO
GO";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(0, scripts.Count(), "Expected no scripts since they would be empty.");
        }

        [Test]
        public void MultiLineQuoteShouldNotBeSplitByGoKeyword()
        {
            string script = "PRINT '" + Environment.NewLine
                            + "GO" + Environment.NewLine
                            + "SELECT * FROM BLAH" + Environment.NewLine
                            + "GO" + Environment.NewLine
                            + "'";

            var scripts = new ScriptSplitter(script);

            Assert.AreEqual(script, scripts.First());
            Assert.AreEqual(1, scripts.Count(), "expected only one script");
        }

        [Test]
        public void MultiLineQuoteShouldNotIgnoreDoubleQuote()
        {
            string script = "PRINT '" + Environment.NewLine
                            + "''" + Environment.NewLine
                            + "GO" + Environment.NewLine
                            + "/*" + Environment.NewLine
                            + "GO"
                            + "'";

            var scripts = new ScriptSplitter(script);

            Assert.AreEqual(1, scripts.Count());
            Assert.AreEqual(script, scripts.First());
        }

        [Test]
        public void ParseScriptParsesCorrectly()
        {
            const string script =
                @"SET QUOTED_IDENTIFIER OFF 
-- Comment
Go
		
SET ANSI_NULLS ON 


GO

GO

SET ANSI_NULLS ON 


CREATE TABLE [<username,varchar,dbo>].[blog_Gost] (
	[HostUserName] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[Password] [nvarchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[Salt] [nvarchar] (32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[DateCreated] [datetime] NOT NULL
) ON [PRIMARY]
gO

";
            List<string> scripts = new ScriptSplitter(script).ToList();
            Assert.AreEqual(3, scripts.Count(), "This should parse to three scripts.");


            for (int i = 0; i < scripts.Count(); i++)
            {
                string sqlScript = scripts[i];
                Assert.IsFalse(sqlScript.StartsWith("GO"), "Script '" + i + "' failed had a GO statement");
            }

            string expectedThirdScriptBeginning = @"SET ANSI_NULLS ON 


CREATE TABLE [<username,varchar,dbo>].[blog_Gost]";

            Assert.AreEqual(expectedThirdScriptBeginning,
                            scripts[2].Substring(0, expectedThirdScriptBeginning.Length),
                            "Script not parsed correctly");
        }

        [Test]
        public void ScriptSplitterCanEnumerate()
        {
            var splitter = new ScriptSplitter("This is a test");
            IEnumerable<string> enumerable = splitter;
            int i = 0;
#pragma warning disable 168
            foreach (var s in enumerable)
#pragma warning restore 168
            {
                i++;
            }
            Assert.AreEqual(1, i);
        }

        [Test]
        public void SemiColonDoesNotSplitScript()
        {
            const string script = "CREATE PROC Blah AS SELECT FOO; SELECT Bar;";
            var scripts = new ScriptSplitter(script);
            Assert.AreEqual(1, scripts.Count(), "Expected no scripts since they would be empty.");
        }

        [Test]
        public void SlashStarCommentAfterGoThrowsException()
        {
            const string script = @"PRINT 'blah'
GO /* blah */";

            // assert
            Assert.Throws<SqlParseException>(() => new ScriptSplitter(script).ToList());
        }
    }
}