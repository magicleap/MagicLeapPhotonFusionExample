using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace MagicLeap.Utils
{
	public static class MathUtils
	{
		private static readonly Matrix4x4 _invertYMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
		private static readonly Matrix4x4 _invertZMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
	#region Matrices Calculation Info

		//Learn about Matrices here: https://github.com/Bunny83/Unity-Articles/blob/master/Matrix%20crash%20course.md

		//  member variables |      indices
		// ------------------|-----------------
		// m00 m01 m02 m03   |   00  04  08  12
		// m10 m11 m12 m13   |   01  05  09  13
		// m20 m21 m22 m23   |   02  06  10  14
		// m30 m31 m32 m33   |   03  07  11  15

	#endregion

	#region Quaternion Average Calculation Info

		//Based off - except with built-in Unity Methods
		//https://forum.unity.com/threads/average-quaternions.86898/

	#endregion

		public static Quaternion ToUnityQuaternionConvert(Quaternion gltfQuat, bool CoordinateSpaceConversionRequiresHandednessFlip, Vector3 flip)
		{
			var fromAxisOfRotation = new Vector3(gltfQuat.x, gltfQuat.y, gltfQuat.z);
			var axisFlipScale = CoordinateSpaceConversionRequiresHandednessFlip ? -1.0f : 1.0f;
			var toAxisOfRotation = axisFlipScale * Vector3.Scale(fromAxisOfRotation, flip);

			return new Quaternion(toAxisOfRotation.x, toAxisOfRotation.y, toAxisOfRotation.z, gltfQuat.w);
		}


		/// <summary>
		/// Converts a quaternion from right-handed coordinates system (OpenCV) to left-handed coordinates system (Unity)
		/// </summary>
		/// <param name="q"> </param>
		/// <returns> </returns>
		public static Quaternion ConvertRightCoordinatesToUnityCoordinates(Quaternion q)
		{
			return new Quaternion(q.x, q.y, -q.z, -q.w);
		}

		public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
		{
			Vector3 scale;
			scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
			scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
			scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;

			if (Vector3.Cross(matrix.GetColumn(0), matrix.GetColumn(1)).normalized != (Vector3)matrix.GetColumn(2).normalized)
			{
				scale.x *= -1;
			}

			return scale;
		}

		/// <summary>
		/// Performs a lowpass check on the position and rotation in newPose, comparing them to oldPose.
		/// </summary>
		/// <param name="oldPose"> Old PoseData. </param>
		/// <param name="newPose"> New PoseData. </param>
		/// <param name="positionThreshold"> Positon threshold. </param>
		/// <param name="rotationThreshold"> Rotation threshold. </param>
		public static void LowpassFilter(Vector3 oldPosition, Quaternion oldRotation, ref Vector3 newPosition, ref Quaternion newRotation, float positionThreshold, float rotationThreshold)
		{
			positionThreshold *= positionThreshold;

			var posDiff = (newPosition - oldPosition).sqrMagnitude;
			var rotDiff = Quaternion.Angle(newRotation, oldRotation);

			if (posDiff < positionThreshold)
			{
				newPosition = oldPosition;
			}

			if (rotDiff < rotationThreshold)
			{
				newRotation = oldRotation;
			}
		}


		public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
		{
			//https://docs.unity3d.com/ScriptReference/Matrix4x4.html

			Vector3 translate;
			translate.x = matrix.m03;
			translate.y = matrix.m13;
			translate.z = matrix.m23;
			return translate;
		}

		[Obsolete("Method is obsolete. use  matrix.rotation instead", false)]
		public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
		{
			//https://answers.unity.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
			//Column index 2 
			var forward = new Vector3(matrix.m02, matrix.m12, matrix.m22);
			if (forward == Vector3.zero)
			{
				return Quaternion.identity;
			}

			//Column index 1 
			var upwards = new Vector3(matrix.m01, matrix.m11, matrix.m21);


			return Quaternion.LookRotation(forward, upwards);
		}

		public static Matrix4x4 ConvertToMatrix(Vector3 position, Quaternion rotation, bool fromOpenCV = false, bool fromOpenGL = false)
		{
			var matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

			// right-handed coordinates system (OpenCV - [X right, Y down, Z forward]) to left-handed one (Unity -  [X right, Y up, Z forward])
			// https://stackoverflow.com/questions/30234945/change-handedness-of-a-row-major-4x4-transformation-matrix
			if (fromOpenCV)
			{
				matrix = _invertYMatrix * matrix * _invertYMatrix;
			}


			// right-handed coordinates system (OpenGL - [X right, Y up, Z backward]) to left-handed one (Unity -  [X right, Y up, Z forward])
			// https://gist.github.com/chrisse27/07da91c22fd638e118f359d024d339e8#file-coordinate-system-transformations-md
			if (fromOpenGL)
			{
				matrix = _invertZMatrix * matrix * _invertZMatrix;
			}

			return matrix;
		}

	#region Quaternion Averages

		public static Quaternion Average(Quaternion reference, Quaternion[] source)
		{
			var referenceInverse = Quaternion.Inverse(reference);

			var result = new Vector3();
			float angle = 0;
			foreach (var q in source)
			{
				(referenceInverse * q).ToAngleAxis(out var angleInDegrees, out var rotationAxis);
				result += rotationAxis;
				angle += angleInDegrees;
			}

			result /= source.Length;
			angle /= source.Length;
			return reference * Quaternion.AngleAxis(angle, result);
		}

		public static Quaternion Average(Quaternion[] source)
		{
			var result = new Vector3();
			float angle = 0;
			foreach (var q in source)
			{
				q.ToAngleAxis(out var angleInDegrees, out var rotationAxis);
				result += rotationAxis;
				angle += angleInDegrees;
			}

			result /= source.Length;
			angle /= source.Length;
			return Quaternion.AngleAxis(angle, result);
		}

		public static Quaternion Average(Quaternion[] source, int iterations)
		{
			Assert.IsFalse(source.Length > 0);
			var reference = Quaternion.identity;
			for (var i = 0; i < iterations; i++)
			{
				reference = Average(reference, source);
			}

			return reference;
		}

	#endregion


	#region Vector Averages

		public static Vector3 Average(Vector3[] source)
		{
			return source.Aggregate(Vector3.zero, (acc, v) => acc + v) / source.Length;
		}

		public static Vector3 Average(Vector3 reference, Vector3[] source)
		{
			return source.Aggregate(reference, (acc, v) => acc + v) / source.Length;
		}

		public static Vector3 Average(Vector3 reference, Vector3[] source, int iterations)
		{
			Assert.IsFalse(source.Length > 0);

			for (var i = 0; i < iterations; i++)
			{
				reference = Average(reference, source);
			}

			return reference;
		}

		public static Vector3 Average(Vector3[] source, int iterations)
		{
			Assert.IsFalse(source.Length > 0);
			var reference = Vector3.zero;
			for (var i = 0; i < iterations; i++)
			{
				reference = Average(reference, source);
			}

			return reference;
		}

	#endregion

		public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
		{
			var dot = Quaternion.Dot(q1, q2);

			if (dot < 0.0f)
			{
				return false;
			}

			return true;
		}

		public static Vector3 AveragePosition(Vector3 pos, ref List<Vector3> avgPos, int samples = 10)
		{
			avgPos.Add(pos);


			if (avgPos.Count > samples)
			{
				avgPos.RemoveAt(0);
			}

			return avgPos.Aggregate(Vector3.zero, (acc, v) => acc + v) / avgPos.Count;
		}

		public static Quaternion AverageRotation(Quaternion rot, ref List<Quaternion> avgRot, int samples = 10)
		{
			avgRot.Add(rot);

			if (avgRot.Count > samples)
			{
				avgRot.RemoveAt(0);
			}

			var newRot = Quaternion.identity;
			var rotHelper = new Vector4(0, 0, 0, 0);

			for (var x = 0; x < avgRot.Count; x++)
			{
				newRot = AverageQuaternion(ref rotHelper, avgRot[x], avgRot[0], x);
			}


			return newRot;
		}

		public static (Vector3, Quaternion) Average(Vector3 pos, Quaternion rot, ref List<Vector3> avgPos, ref List<Quaternion> avgRot, int samples = 10)
		{
			avgPos.Add(pos);
			avgRot.Add(rot);

			if (avgPos.Count > samples)
			{
				avgPos.RemoveAt(0);
				avgRot.RemoveAt(0);
			}


			var newPos = new Vector3(0, 0, 0);
			var newRot = Quaternion.identity;
			var rotHelper = new Vector4(0, 0, 0, 0);

			for (var x = 0; x < avgPos.Count; x++)
			{
				newPos += avgPos[x];
				newRot = AverageQuaternion(ref rotHelper, avgRot[x], avgRot[0], x);
			}

			newPos /= avgPos.Count;
			return (newPos, newRot);
		}

		//Get an average (mean) from more then two quaternions (with two, slerp would be used).
		//Note: this only works if all the quaternions are relatively close together.
		//Usage: 
		//-Cumulative is an external Vector4 which holds all the added x y z and w components.
		//-newRotation is the next rotation to be added to the average pool
		//-firstRotation is the first quaternion of the array to be averaged
		//-addAmount holds the total amount of quaternions which are currently added
		//This function returns the current average quaternion
		public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount)
		{
			var w = 0.0f;
			var x = 0.0f;
			var y = 0.0f;
			var z = 0.0f;

			//Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
			//q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
			if (!AreQuaternionsClose(newRotation, firstRotation))
			{
				newRotation = InverseSignQuaternion(newRotation);
			}

			//Average the values
			var addDet = 1f / addAmount;
			cumulative.w += newRotation.w;
			w = cumulative.w * addDet;
			cumulative.x += newRotation.x;
			x = cumulative.x * addDet;
			cumulative.y += newRotation.y;
			y = cumulative.y * addDet;
			cumulative.z += newRotation.z;
			z = cumulative.z * addDet;

			//note: if speed is an issue, you can skip the normalization step
			return NormalizeQuaternion(x, y, z, w);
		}

		public static Quaternion NormalizeQuaternion(float x, float y, float z, float w)
		{
			var lengthD = 1.0f / (w * w + x * x + y * y + z * z);
			w *= lengthD;
			x *= lengthD;
			y *= lengthD;
			z *= lengthD;

			return new Quaternion(x, y, z, w);
		}


		//Changes the sign of the quaternion components. This is not the same as the inverse.
		public static Quaternion InverseSignQuaternion(Quaternion q)
		{
			return new Quaternion(-q.x, -q.y, -q.z, -q.w);
		}
	}
}
