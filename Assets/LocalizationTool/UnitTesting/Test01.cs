using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Test01
{ 
	public class Test01
	{
    	
    		[Test]
    		public void Test01SimplePasses()
    		{
        		// Arrange
			
			// Act

			// Assert
    		}

    	
    		[UnityTest]
    		public IEnumerator Test01WithEnumeratorPasses()
    		{
        		// Arrange
		
			// Act

			// Assert

        		yield return null;
    		}
	}
}
