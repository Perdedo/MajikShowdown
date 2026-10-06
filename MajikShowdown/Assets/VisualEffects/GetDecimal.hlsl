float2 GetSDecimal(float decimal)
{
    float x = floor(decimal * 10);
    float y =  10 * ((decimal * 10) - x );
    float2 vec = float2(x / 10, y);
    return vec;
}