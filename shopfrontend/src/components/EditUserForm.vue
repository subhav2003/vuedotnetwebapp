<template>
  <div class="fixed inset-0 bg-black bg-opacity-60 z-50 flex items-center justify-center">
    <Toast :toasts="toasts" @close="removeToast" />

    <div class="bg-[#1a1a1a] text-[#C8B280] p-6 rounded-lg shadow-xl w-full max-w-lg overflow-auto max-h-[90vh] relative border border-[#333]">
      <!-- Close Button -->
      <button
          @click="$emit('close')"
          class="absolute top-3 right-3 text-[#888] hover:text-red-600 text-xl font-bold"
          title="Close"
      >
        X
      </button>

      <!-- Header -->
      <h2 class="text-2xl font-bold mb-6 text-[#F5DEB3]">Edit Profile</h2>

      <!-- Form -->
      <form @submit.prevent="saveProfile" class="space-y-4">
        <div v-for="f in fields" :key="f.name">
          <label :for="f.name" class="block mb-1 font-medium">{{ f.label }}</label>
          <input
              :id="f.name"
              :type="f.type"
              v-model="form[f.name]"
              class="w-full p-2 rounded bg-[#121212] border border-[#3a3a3a] text-[#C8B280] focus:outline-none focus:ring focus:ring-[#A0522D]"
              required
          />
        </div>

        <!-- Gender -->
        <div>
          <label class="block font-medium mb-1">Gender</label>
          <select
              v-model="form.gender"
              class="w-full p-2 rounded bg-[#121212] border border-[#3a3a3a] text-[#C8B280]"
          >
            <option value="">Select Gender</option>
            <option value="male">Male</option>
            <option value="female">Female</option>
            <option value="other">Other</option>
          </select>
        </div>

        <!-- Date of Birth -->
        <div>
          <label class="block font-medium mb-1">Date of Birth</label>
          <input
              type="date"
              v-model="form.dateOfBirth"
              class="w-full p-2 rounded bg-[#121212] border border-[#3a3a3a] text-[#C8B280]"
          />
        </div>

        <!-- Password -->
        <div>
          <label class="block font-medium mb-1">New Password (optional)</label>
          <input
              type="password"
              v-model="form.password"
              class="w-full p-2 rounded bg-[#121212] border border-[#3a3a3a] text-[#C8B280]"
          />
        </div>

        <div>
          <label class="block font-medium mb-1">Confirm New Password</label>
          <input
              type="password"
              v-model="form.passwordConfirmation"
              class="w-full p-2 rounded bg-[#121212] border border-[#3a3a3a] text-[#C8B280]"
          />
        </div>

        <!-- Submit -->
        <div class="flex justify-end mt-6">
          <button
              type="submit"
              :disabled="saving"
              class="bg-[#A0522D] text-white px-6 py-2 rounded hover:bg-[#8B4513] disabled:opacity-50"
          >
            {{ saving ? 'Updating…' : 'Update Profile' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const emit = defineEmits(['close', 'updated'])

const api = process.env.VUE_APP_API_URL || 'https://localhost:5001'
const token = localStorage.getItem('authToken')

// Toast
const toasts = reactive([])
function toast(message, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(() => removeToast(id), 4000)
}
function removeToast(id) {
  const index = toasts.findIndex(t => t.id === id)
  if (index !== -1) toasts.splice(index, 1)
}

// Fields
const fields = [
  { name: 'name',     label: 'Name',     type: 'text'  },
  { name: 'username', label: 'Username', type: 'text'  },
  { name: 'email',    label: 'Email',    type: 'email' },
  { name: 'phone',    label: 'Phone',    type: 'text'  }
]

// Form model
const form = reactive({
  name: '',
  username: '',
  email: '',
  phone: '',
  gender: '',
  dateOfBirth: '',
  password: '',
  passwordConfirmation: ''
})

const saving = ref(false)

// Fetch profile
async function fetchUserProfile() {
  try {
    const res = await axios.get(`${api}/api/account/profile`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    const user = res.data.user
    form.name                 = user.name || ''
    form.username             = user.username || ''
    form.email                = user.email || ''
    form.phone                = user.phone || ''
    form.gender               = user.gender || ''
    form.dateOfBirth          = user.dateOfBirth?.substring(0, 10) || ''
    form.password             = ''
    form.passwordConfirmation = ''
  } catch (err) {
    toast('Failed to load user profile', 'error')
  }
}

// Update profile
async function saveProfile() {
  saving.value = true
  try {
    const payload = {
      Name: form.name,
      Username: form.username,
      Email: form.email,
      Phone: form.phone,
      Gender: form.gender,
      DateOfBirth: form.dateOfBirth || null,
      Password: form.password || undefined,
      PasswordConfirmation: form.passwordConfirmation || undefined
    }

    const res = await axios.put(`${api}/api/account/profile`, payload, {
      headers: { Authorization: `Bearer ${token}` }
    })

    toast(res.data.message || 'Profile updated')
    emit('updated', res.data.user)
    await fetchUserProfile() // refresh with latest values
  } catch (err) {
    toast(err.response?.data?.message || 'Update failed', 'error')
  } finally {
    saving.value = false
  }
}

onMounted(fetchUserProfile)
</script>

<style scoped>
input,
select {
  transition: border-color 0.3s ease, box-shadow 0.3s ease;
}
</style>
