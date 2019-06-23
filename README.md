# Bangumi UWP

一个使用 [Bangumi API](https://github.com/bangumi/api) 开发的 [Bangumi 番組計画](https://bgm.tv) 第三方客户端。

## 功能

- 支持用户登录
- 支持查看用户进度
- 支持查看用户收藏（每类限制25个）
- 支持查看时间表
- 支持搜索
- 详情页支持修改收藏和条目状态、评分和吐槽（不支持删除收藏）

## 安装

### 前提

- Windows 10 16299 及以上
- 打开开发者模式

### 安装脚本

- 下载 [release](https://github.com/Teachoc/Bangumi/releases) 中的 zip 压缩包并解压
- 右键点击 `Add-AppDevPackage.ps1`
- 选择 `使用 PowerShell 运行`
- 根据提示进行安装

## 编译环境

- Visual Studio Community 2019

    说明：编译之前，请至 [Bangumi 开发者平台](https://bgm.tv/dev/app) 创建应用，将申请到的 App ID 等填入 Bangumi.Api/Constants.cs 中。