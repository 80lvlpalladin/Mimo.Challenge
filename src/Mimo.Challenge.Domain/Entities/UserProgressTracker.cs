namespace Mimo.Challenge.Domain.Entities;

public class UserProgressTracker : Dictionary<uint, CourseProgressTracker>
{
    internal UserProgressTracker(uint userId)
    {
        UserId = userId;
    }
    
    public uint UserId { get; }
    
    public uint? FirstStartedLessonTimestamp => 
        Values.Min(chapter => chapter.FirstStartedLessonTimestamp);
    public uint? LastCompletedLessonTimestamp => 
        Values.Max(chapter => chapter.LastCompletedLessonTimestamp);
    
    public uint? FirstStartedChapterTimestamp => 
        Values.Min(course => course.FirstStartedChapterTimestamp);
    
    public uint? LastCompletedChapterTimestamp =>
        Values.Min(course => course.LastCompletedChapterTimestamp);
    
    public int TotalLessonsCompletedCount => 
        Values.Sum(course => course.TotalLessonsCompletedCount);
    
    public int TotalChaptersCompletedCount => 
        Values.Sum(course => course.CompletedChaptersCount);
    
    public int CompletedCoursesCount => 
        Values.Count(course => course.PercentageCompleted == 100);
    
    public uint? LastCompletedCourseTimestamp { get; private set; }

    public bool TryMarkLessonCompleted
        (uint courseId, uint chapterId, uint lessonId, uint startedTimestamp, uint completedTimestamp)
    {
        var courseProgressTracker = this[courseId];
        
        var lessonCompletedResult = 
            courseProgressTracker.TryMarkLessonCompleted
                (chapterId, lessonId, startedTimestamp, completedTimestamp);
        
        if(lessonCompletedResult == false)
            return lessonCompletedResult;
        
        if(courseProgressTracker.PercentageCompleted == 100)
            LastCompletedCourseTimestamp = completedTimestamp;

        return lessonCompletedResult;
    }
}

public class CourseProgressTracker : Dictionary<uint, ChapterProgressTracker>
{
    internal CourseProgressTracker()
    {
        
    }
    
    public uint? FirstStartedLessonTimestamp => 
        Values.Min(chapter => chapter.FirstStartedLessonTimestamp);
    
    public uint? LastCompletedLessonTimestamp => 
        Values.Max(chapter => chapter.LastCompletedLessonTimestamp);
    
    public uint? FirstStartedChapterTimestamp => FirstStartedLessonTimestamp;
    
    public uint? LastCompletedChapterTimestamp { get; private set; }
    
    public int TotalLessonsCompletedCount =>
        Values.Sum(chapter => chapter.LessonsCompletedCount);
    
    public int CompletedChaptersCount => 
        Values.Count(chapter => chapter.PercentageCompleted == 100);
    
    public bool TryMarkLessonCompleted
        (uint chapterId, uint lessonId, uint startedTimestamp, uint completedTimestamp)
    {
        var chapterProgressTracker = this[chapterId];
        
        var lessonCompletedResult = 
            chapterProgressTracker.TryMarkLessonCompleted(lessonId, startedTimestamp, completedTimestamp);
        
        if(lessonCompletedResult == false)
            return lessonCompletedResult;
        
        if(chapterProgressTracker.PercentageCompleted == 100)
            LastCompletedChapterTimestamp = completedTimestamp;

        return lessonCompletedResult;
    }

    public ushort PercentageCompleted
    {
        get
        {
            var totalChaptersCount = Keys.Count;
            return Convert.ToUInt16((float) CompletedChaptersCount / totalChaptersCount * 100);
        }
    }
}

public class ChapterProgressTracker : HashSet<uint>
{
    private readonly int _initialLessonCount;
    private uint _firstStartedLessonTimestamp = uint.MaxValue;
    private uint _lastCompletedLessonTimestamp = uint.MinValue;

    internal ChapterProgressTracker(IReadOnlyCollection<uint> lessonIds) : base(lessonIds)
    {
        _initialLessonCount = lessonIds.Count;
    }

    public uint? FirstStartedLessonTimestamp
    {
        get
        {
            if(_firstStartedLessonTimestamp == uint.MaxValue)
                return null;
            
            return _firstStartedLessonTimestamp;
        }
    }

    public uint? LastCompletedLessonTimestamp
    {
        get
        {
            if(_lastCompletedLessonTimestamp == uint.MinValue)
                return null;
            
            return _lastCompletedLessonTimestamp;
        }
    }


    /// <returns>If successful</returns>
    public bool TryMarkLessonCompleted(uint lessonId, uint startedTimestamp, uint completedTimestamp)
    {
        var removeResult = Remove(lessonId);

        if (removeResult == false)
            return removeResult;
        
        if(startedTimestamp < _firstStartedLessonTimestamp)
            _firstStartedLessonTimestamp = startedTimestamp;
        
        if(completedTimestamp > _lastCompletedLessonTimestamp)
            _lastCompletedLessonTimestamp = completedTimestamp;

        return removeResult;
    }

    public ushort PercentageCompleted => 
        Convert.ToUInt16((float) (_initialLessonCount - Count) / _initialLessonCount * 100);
    
    public int LessonsCompletedCount => _initialLessonCount - Count;
    
}