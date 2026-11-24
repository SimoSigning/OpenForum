
using System.Collections.Concurrent;

public class ChatConnectionTracker
{
    private readonly object _lock = new();

    // User ID -> connection IDs
    private readonly Dictionary<string, HashSet<string>> _userConnections = new();

    // Connection ID -> joined room IDs
    private readonly Dictionary<string, HashSet<int>> _connectionRooms = new();

    public List<string> GetConnections(string userId)
    {
        lock (_lock)
        {
            return _userConnections.TryGetValue(userId, out var connections)
                ? connections.ToList()
                : new List<string>();
        }
    }

    public void AddConnection(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
            {
                connections = new HashSet<string>();
                _userConnections[userId] = connections;
            }

            connections.Add(connectionId);

            if (!_connectionRooms.ContainsKey(connectionId))
            {
                _connectionRooms[connectionId] = new HashSet<int>();
            }
        }
    }

    public void RemoveConnection(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _userConnections.Remove(userId);
                }
            }

            _connectionRooms.Remove(connectionId);
        }
    }

    public List<int> RemoveConnectionAndGetLastRooms(
    string userId,
    string connectionId)
    {
        lock (_lock)
        {
            var joinedRooms = _connectionRooms.TryGetValue(
                connectionId,
                out var rooms)
                    ? rooms.ToList()
                    : new List<int>();

            _connectionRooms.Remove(connectionId);

            if (!_userConnections.TryGetValue(userId, out var connections))
                return joinedRooms;

            connections.Remove(connectionId);

            if (connections.Count == 0)
            {
                _userConnections.Remove(userId);
                return joinedRooms;
            }

            // Only return rooms that no remaining connection for this user
            // is currently joined to.
            return joinedRooms
                .Where(roomId => !connections.Any(otherConnectionId =>
                    _connectionRooms.TryGetValue(
                        otherConnectionId,
                        out var otherRooms)
                    && otherRooms.Contains(roomId)))
                .ToList();
        }
    }

    public void AddRoom(string connectionId, int roomId)
    {
        lock (_lock)
        {
            if (!_connectionRooms.TryGetValue(connectionId, out var rooms))
            {
                rooms = new HashSet<int>();
                _connectionRooms[connectionId] = rooms;
            }

            rooms.Add(roomId);
        }
    }

    public void RemoveRoom(string connectionId, int roomId)
    {
        lock (_lock)
        {
            if (_connectionRooms.TryGetValue(connectionId, out var rooms))
            {
                rooms.Remove(roomId);
            }
        }
    }

    public List<int> GetRooms(string connectionId)
    {
        lock (_lock)
        {
            return _connectionRooms.TryGetValue(connectionId, out var rooms)
                ? rooms.ToList()
                : new List<int>();
        }
    }

    public bool HasOtherConnectionInRoom(
        string userId,
        string connectionId,
        int roomId)
    {
        lock (_lock)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
                return false;

            return connections
                .Where(id => id != connectionId)
                .Any(id =>
                    _connectionRooms.TryGetValue(id, out var rooms)
                    && rooms.Contains(roomId));
        }
    }
}
