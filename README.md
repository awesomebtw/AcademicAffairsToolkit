# 教务工具箱 Academic affairs toolkit
这是吉林大学“C#程序设计”课设作品，**请不要用于实际工作中**。
This project is a course design for "C# programming language", Jilin University.
**Do not use for actual work.**
使用 .NET Core 3.1 和 C# 8.0。
## 课设要求
本课程设计采用团队方式利用C#开发一款辅助教务工作的软件，主要包含督学课表生成，工作量计算和监考分配三个模块。
3.监考分配策略（40%）
根据安排好时间地点的监考信息表（监考.xlsx），将监考任务分配给每个教研室，每个教研室承担监考任务量应根据教研室人数确定，根据每个考场考生人数可以计算出监考所需老师总人次，从而可得到学院监考老师平均监考次数，最终得到某个教研室需要监考总人次，据此进行分配。具体要求如下。
- 具有简洁美观的UI，选择监考信息表，支持xls和xlsx两种格式；
- 考生人数70人以下，安排2人监考；
- 考生人数70—100人，安排3人监考；
- 考生人数100人以上，安排4人监考；
- 考生人数150以上，安排5人监考；
- 考生人数180以上，安排6人监考；
- 同一时间段不能安排超过教研室总人数（教研室.xlsx）的监考（时间约束）；
- 尽量同一教研室监考平均分布在考试周，不要集中在某一天；
- 提供排除功能，界面中可以设置某天某个时间段不安排某个教研室监考；
- 用教研室主任名字+人数补全监考信息表。
## 如何工作
利用遗传算法生成满足要求的教研室的序列。使用的交叉算子均匀交叉，变异算子为简单倒位变异。
## 使用的第三方库和资源
- ExcelMapper
- Extended Wpf Toolkit (dotnetproject 版)
- Fluent.Ribbon
- RangeTree
- Visual Studio 2019 Image Library