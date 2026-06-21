CREATE DATABASE AIInterviewSimulatorDb;
GO
USE AIInterviewSimulatorDb;
GO
CREATE TABLE InterviewSessions (Id uniqueidentifier PRIMARY KEY, Topic nvarchar(100) NOT NULL, Difficulty nvarchar(50) NOT NULL, CreatedAtUtc datetime2 NOT NULL, CompletedAtUtc datetime2 NULL, IsCompleted bit NOT NULL);
CREATE TABLE InterviewQuestions (Id int IDENTITY PRIMARY KEY, InterviewSessionId uniqueidentifier NOT NULL, QuestionNumber int NOT NULL, Concept nvarchar(100) NOT NULL, Difficulty nvarchar(50) NOT NULL, Text nvarchar(max) NOT NULL, CreatedAtUtc datetime2 NOT NULL, CONSTRAINT FK_Questions_Sessions FOREIGN KEY (InterviewSessionId) REFERENCES InterviewSessions(Id) ON DELETE CASCADE);
CREATE TABLE InterviewAnswers (Id int IDENTITY PRIMARY KEY, InterviewQuestionId int NOT NULL UNIQUE, Text nvarchar(max) NOT NULL, SubmittedAtUtc datetime2 NOT NULL, CONSTRAINT FK_Answers_Questions FOREIGN KEY (InterviewQuestionId) REFERENCES InterviewQuestions(Id) ON DELETE CASCADE);
CREATE TABLE AnswerEvaluations (Id int IDENTITY PRIMARY KEY, InterviewAnswerId int NOT NULL UNIQUE, Score int NOT NULL CHECK (Score BETWEEN 0 AND 100), Strengths nvarchar(max) NOT NULL, Weaknesses nvarchar(max) NOT NULL, ImprovementSuggestion nvarchar(max) NOT NULL, CONSTRAINT FK_Evaluations_Answers FOREIGN KEY (InterviewAnswerId) REFERENCES InterviewAnswers(Id) ON DELETE CASCADE);
