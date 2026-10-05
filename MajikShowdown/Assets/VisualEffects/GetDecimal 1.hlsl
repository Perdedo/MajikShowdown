void GetSDecimal(float decimal, out Vector2 vec)
{
    float y = floor(decimal * 10);
    float x = y - floor(decimal * 10);
    vec = Vector2(y,x);
    return vec;
}