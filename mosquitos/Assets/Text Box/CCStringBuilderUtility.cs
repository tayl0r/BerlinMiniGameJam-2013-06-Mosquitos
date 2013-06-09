// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0

using System.Text;
using UnityEngine;

/// <summary>
/// Utility class for efficient and fast StringBuilder manipulation.
/// None of the methods create temporary or persistent objects.
/// Memory allocation only happens when the StringBuilder needs more space.
/// </summary>
public static class CCStringBuilderUtility {
	
	/// <summary>
	/// Character used as decimal separator.
	/// Change this if you don't want to use '.'.
	/// </summary>
	public static char decimalSeparator = '.';
	
	/// <summary>
	/// Character used as digit group separatator.
	/// Change this is you don't want to use ','.
	/// </summary>
	public static char groupSeparator = ',';
	
	/// <summary>
	/// Character used as padding.
	/// Change this is you don't want to use '0'.
	/// </summary>
	public static char padding = '0';
	
	/// <summary>
	/// Append an int to a StringBuilder.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// The int to append.
	/// </param>
	public static void AppendInt (StringBuilder s, int number) {
		if(number < 0){
			s.Append('-');
			number = -number;
		}
		int firstIndex = s.Length;
		do{
			s.Append((char)(number % 10 + '0'));
			number /= 10;
		}
		while(number > 0);
		Reverse(s, firstIndex, s.Length - 1);
	}
	
	/// <summary>
	/// Append an integer to a StringBuilder and include left padding.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// An int.
	/// </param>
	/// <param name="digitCount">
	/// How many digits should be shown. If number has fewer digits, it will be padded to the left.
	/// </param>
	public static void AppendInt (StringBuilder s, int number, int digitCount) {
		if(number < 0){
			s.Append('-');
			number = -number;
		}
		int
			firstIndex = s.Length,
			length = 0;
		do{
			s.Append((char)(number % 10 + '0'));
			number /= 10;
			length += 1;
		}
		while(number > 0);
		while(length++ < digitCount){
			s.Append('0');
		}
		Reverse(s, firstIndex, s.Length - 1);
	}

	/// <summary>
	/// Append an integer to a StringBuilder and include digit group separators.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// An int.
	/// </param>
	public static void AppendIntGrouped (StringBuilder s, int number) {
		if(number < 0){
			s.Append('-');
			number = -number;
		}
		int
			firstIndex = s.Length,
			separator = -1;
		do{
			if(++separator == 3){
				s.Append(groupSeparator);
				separator = 0;
			}
			s.Append((char)(number % 10 + '0'));
			number /= 10;
		}
		while(number > 0);
		Reverse(s, firstIndex, s.Length - 1);
	}
	
	/// <summary>
	/// Append an integer to a StringBuilder and include both left padding and digit group separators.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// An int.
	/// </param>
	/// <param name="digitCount">
	/// How many digits should be shown. If number has fewer digits, it will be padded to the left.
	/// </param>
	public static void AppendIntGrouped (StringBuilder s, int number, int digitCount) {
		if(number < 0){
			s.Append('-');
			number = -number;
		}
		int
			firstIndex = s.Length,
			length = 0,
			separator = -1;
		do{
			if(++separator == 3){
				s.Append(groupSeparator);
				separator = 0;
			}
			s.Append((char)(number % 10 + '0'));
			number /= 10;
			length += 1;
		}
		while(number > 0);
		while(length++ < digitCount){
			if(++separator == 3){
				s.Append(groupSeparator);
				separator = 0;
			}
			s.Append('0');
		}
		Reverse(s, firstIndex, s.Length - 1);
	}
	
	/// <summary>
	/// Append a float to a Stringbuilder.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// A float.
	/// </param>
	/// <param name="decimalCount">
	/// How many decimals to show.
	/// </param>
	public static void AppendFloat (StringBuilder s, float number, int decimalCount) {
		int n = (int)number;
		AppendInt(s, n);
		if(decimalCount <= 0){
			return;
		}
		s.Append(decimalSeparator);
		number -= n;
		for(n = 0; n < decimalCount; n++){
			number *= 10;
		}
		n = Mathf.FloorToInt(number);
		AppendInt(s, n < 0 ? -n : n, decimalCount);
	}
	
	/// <summary>
	/// Append an integer to a StringBuilder and include left padding.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// A float.
	/// </param>
	/// <param name="decimalCount">
	/// How many decimals to show.
	/// </param>
	/// <param name="digitCount">
	/// How many digits should be shown. If number's integer part has fewer digits, it will be padded to the left.
	/// </param>
	public static void AppendFloat (StringBuilder s, float number, int decimalCount, int digitCount) {
		int n = (int)number;
		AppendInt(s, n, digitCount);
		if(decimalCount <= 0){
			return;
		}
		s.Append(decimalSeparator);
		number -= n;
		for(n = 0; n < decimalCount; n++){
			number *= 10;
		}
		n = Mathf.FloorToInt(number);
		AppendInt(s, n < 0 ? -n : n, decimalCount);
	}
	
	/// <summary>
	/// Append a float to a Stringbuilder and include digit group separators.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// A float.
	/// </param>
	/// <param name="decimalCount">
	/// How many decimals to show.
	/// </param>
	public static void AppendFloatGrouped (StringBuilder s, float number, int decimalCount) {
		int n = (int)number;
		AppendIntGrouped(s, n);
		if(decimalCount <= 0){
			return;
		}
		s.Append(decimalSeparator);
		number -= n;
		for(n = 0; n < decimalCount; n++){
			number *= 10;
		}
		n = Mathf.FloorToInt(number);
		AppendInt(s, n < 0 ? -n : n, decimalCount);
	}
	
	/// <summary>
	/// Append an integer to a StringBuilder and include both left padding and digit group separators.
	/// </summary>
	/// <param name="s">
	/// A <see cref="StringBuilder"/>.
	/// </param>
	/// <param name="number">
	/// A float.
	/// </param>
	/// <param name="decimalCount">
	/// How many decimals to show.
	/// </param>
	/// <param name="digitCount">
	/// How many digits should be shown. If number's integer part has fewer digits, it will be padded to the left.
	/// </param>
	public static void AppendFloatGrouped (StringBuilder s, float number, int decimalCount, int digitCount) {
		int n = (int)number;
		AppendIntGrouped(s, n, digitCount);
		if(decimalCount <= 0){
			return;
		}
		s.Append(decimalSeparator);
		number -= n;
		for(n = 0; n < decimalCount; n++){
			number *= 10;
		}
		n = Mathf.FloorToInt(number);
		AppendInt(s, n < 0 ? -n : n, decimalCount);
	}
	
	/// <summary>
	/// Reverse a portion of a StringBuilder.
	/// </summary>
	/// <param name="s">
	/// The <see cref="StringBuilder"/> to modify.
	/// </param>
	/// <param name="firstIndex">
	/// Index of the first character to reverse.
	/// </param>
	/// <param name="lastIndex">
	/// Index of the last character to reverse.
	/// </param>
	public static void Reverse (StringBuilder s, int firstIndex, int lastIndex) {
		while(firstIndex < lastIndex){
			char c = s[firstIndex];
			s[firstIndex++] = s[lastIndex];
			s[lastIndex--] = c;
		}
	}
}
