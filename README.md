## 关于这个项目
* 已实现：
    * Chunk的基础绘制（使用基本的漫反射模型，无贴图）
    * Chunk的基本挖掘操作（球体挖掘函数表现很差）
    * 地形的基础生成（使用fbm）
* 已知BUG:
    * 暂无
* 正在做：
    * Chunk的LOD状态更新
* 其他：
    * Chunk的实际显示大小应为16^3，为保证边界法线正确，计算大小为18^3，目前显示大小仍为18^3
    * 地形的平滑效果不佳（细分着色器无法使用，特定情况下在几何着色器中增加平滑三角形输出可能会改善情况）
    * 在Unity引擎的环境下难以实现Shader即时修改
    * 对于地形区块实现了一个简单的缓存池
    * 更多细节详见[BLOG](https://al0ha0e.github.io/)

## 地形的基础生成效果
    ![](/ScreenSht/Perlin_1.png)
