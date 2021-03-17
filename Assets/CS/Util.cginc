#ifndef UTIL
    #define UTIL

    static const int2 offset2d[] =
    {
        int2(-1,-1),
        int2(0,-1),
        int2(1,-1),

        int2(-1,0),
        int2(0,0),
        int2(1,0),
        
        int2(-1,1),
        int2(0,1),
        int2(1,1)
    };

    static const int3 offset3d[] =
    {
        int3(-1,-1,-1),
        int3(0,-1,-1),
        int3(1,-1,-1),
        int3(-1,0,-1),
        int3(0,0,-1),
        int3(1,0,-1),
        int3(-1,1,-1),
        int3(0,1,-1),
        int3(1,1,-1),

        int3(-1,-1,0),
        int3(0,-1,0),
        int3(1,-1,0),
        int3(-1,0,0),
        int3(0,0,0),
        int3(1,0,0),
        int3(-1,1,0),
        int3(0,1,0),
        int3(1,1,0),

        int3(-1,-1,1),
        int3(0,-1,1),
        int3(1,-1,1),
        int3(-1,0,1),
        int3(0,0,1),
        int3(1,0,1),
        int3(-1,1,1),
        int3(0,1,1),
        int3(1,1,1)
    };

    bool IsIndexInRange2D(int2 index, int res)
    {
        return index.x >= 0 && index.x < res&& index.y >= 0 && index.y < res;
    }

    bool IsIndexInRange3D(int3 index, int res)
    {
        return index.x >= 0 && index.x < res&& index.y >= 0 && index.y < res&& index.z >= 0 && index.z < res;
    }

    float4 SetChannel(int channel, float src, float4 target)
    {
        float4 result = target;
        if(channel == 0)
        {
            result.x = src;
        }
        else if(channel == 1)
        {
            result.y = src;
        }
        else if(channel == 2)
        {
            result.z = src;
        }
        else if(channel == 3)
        {
            result.w = src;
        }
        else
        {
            result = float4(src, src, src, src);
        }
        
        return result;
    }

    float GetChannel(int channel, float4 target)
    {
        if(channel == 0)
        {
            return target.x;
        }
        else if(channel == 1)
        {
            return target.y;
        }
        else if(channel == 2)
        {
            return target.z;
        }
        else
        {
            return target.w;
        }
    }

    int ExpandIndex(int3 idx, int resolution)
    {
        return idx.x + (idx.y + idx.z * resolution) * resolution;
    }

#endif