#!/bin/bash
"""
构建脚本
该脚本用于在构建时生成version.json文件并执行构建操作
"""

# 项目根目录
PROJECT_ROOT=$(dirname "$(readlink -f "$0")")
cd "$PROJECT_ROOT"

echo "=== 开始构建流程 ==="

# 1. 生成版本信息
echo "\n1. 生成版本信息..."
if command -v python3 &> /dev/null; then
    python3 build_version.py
elif command -v python &> /dev/null; then
    python build_version.py
else
    echo "错误: 未找到Python解释器，无法生成版本信息！"
    exit 1
fi

# 检查版本文件是否生成成功
if [ ! -f "version.json" ]; then
    echo "错误: 版本文件生成失败！"
    exit 1
fi

echo "版本信息生成成功！"

# 2. 验证构建结果
echo "\n2. 验证构建结果..."

# 检查版本文件是否存在且有效
if [ -f "version.json" ]; then
    echo "版本文件存在:"
    cat version.json
else
    echo "错误: 版本文件不存在！"
    exit 1
fi

# 检查构建目录是否存在
if [ -d "build" ]; then
    echo "构建目录存在，构建文件:"
    ls -la build/
else
    echo "警告: 构建目录不存在，可能是因为未执行Godot导出。"
fi

echo "\n=== 构建流程完成 ==="
echo "构建时间: $(date '+%Y-%m-%d %H:%M:%S')"
